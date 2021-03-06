﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using TinyConfig.Marshallers;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig
{
    public class ConfigAccessor
    {
        enum ValueMarshallerType
        {
            EXACT,
            MULTIPLE,
        }

        static readonly IReadOnlyDictionary<ValueMarshallerType, IEnumerable<ValueMarshaller>> _standardValueMarshallers
            = new Dictionary<ValueMarshallerType, IEnumerable<ValueMarshaller>>()
            {
                {
                    ValueMarshallerType.EXACT,
                    new ValueMarshaller[]
                    {
                        new BooleanMarshaller(),
                        new SByteMarshaller(),
                        new ByteMarshaller(),
                        new Int16Marshaller(),
                        new UInt16Marshaller(),
                        new Int32Marshaller(),
                        new UInt32Marshaller(),
                        new Int64Marshaller(),
                        new UInt64Marshaller(),
                        new SingleMarshaller(),
                        new DoubleMarshaller(),
                        new StringMarshaller(),
                        new DateTimeMarshaller(),
                        new DateTimeOffsetMarshaller(),
                        new TimeSpanMarshaller(),
                        new MemoryStreamMarshaller(),
                    }
                },
                {
                    ValueMarshallerType.MULTIPLE,
                    new ValueMarshaller[]
                    {
                        new EnumMarshaller(),
                        new ListMarshaller()
                    }
                }
            }.AsReadOnly();
        readonly IEnumerable<ObjectMarshaller> _standarsObjectMarshallers = new ObjectMarshaller[]
        {
             new FlatStructObjectMarshaller()
        };

        readonly Dictionary<ValueMarshallerType, List<ValueMarshaller>> _userValueMarshallers
            = new Dictionary<ValueMarshallerType, List<ValueMarshaller>>()
            {
                {
                    ValueMarshallerType.EXACT,
                    new List<ValueMarshaller>()
                },
                {
                    ValueMarshallerType.MULTIPLE,
                    new List<ValueMarshaller>()
                }
            };
        readonly List<ObjectMarshaller> _userObjectMarshallers = new List<ObjectMarshaller>();
        readonly ConfigReaderWriter _config;
        readonly HashSet<string> _proxyPaths = new HashSet<string>();

        public ConfigSourceInfo SourceInfo { get; }

        internal ConfigAccessor(ConfigReaderWriter config, ConfigSourceInfo configSourceInfo)
        {
            _config = config;
            SourceInfo = configSourceInfo;
        }

        public static bool IsValueSupported(Type type)
        {
            return GetStandartValueMarshaller(type) != null;
        }
        public static ValueMarshaller GetStandartValueMarshaller(Type type)
        {
            return _standardValueMarshallers.Values.Flatten().FirstOrDefault(m => m.IsTypeSupported(type));
        }

        public ConfigAccessor Clear()
        {
            if (_config.KVPs.IsReadOnly)
            {
                throw new InvalidOperationException();
            }

            _config.KVPs.Clear();

            return this;
        }

        public ConfigAccessor Close()
        {
            _config.Close();

            return this;
        }

        public ConfigAccessor AddMarshaller<T>() 
            where T : ValueMarshaller, new()
        {
            if (typeof(ExactValueMarshaller).IsAssignableFrom(typeof(T)))
            {
                _userValueMarshallers[ValueMarshallerType.EXACT].Insert(0, new T());
            }
            else
            {
                _userValueMarshallers[ValueMarshallerType.MULTIPLE].Insert(0, new T());
            }

            return this;
        }
        public ConfigAccessor AddObjectMarshaller<T>()
            where T : ObjectMarshaller, new()
        {
            _userObjectMarshallers.Insert(0, new T());

            return this;
        }

        public ConfigProxy<T> Read<T>(T fallbackValue, [CallerMemberName]string key = "")
        {
            return ReadFrom(fallbackValue, null, key);
        }
        public ConfigProxy<T> ReadFrom<T>(T fallbackValue, string subsection, [CallerMemberName]string key = "")
        {
            ConfigProxy<T> value = null;
            var valueType = typeof(T).IsArray
                ? typeof(T).GetElementType()
                : typeof(T);
            if (GetValueMarshaller(valueType) != null)
            {
                value = ReadValueFrom(fallbackValue, subsection, key);
            }
            else if (GetObjectMarshaller(typeof(T)) != null)
            {
                value = ReadObjectFrom(fallbackValue, subsection, key);
            }
            else
            {
                value = new ConfigProxy<T>(fallbackValue, null, false, delegate { }, delegate { }, delegate { });
            }

            return value;
        }

        public ConfigProxy<T> ReadValue<T>(T fallbackValue, [CallerMemberName]string key = "")
        {
            return ReadValueFrom(fallbackValue, null, key);
        }
        public ConfigProxy<T> ReadValueFrom<T>(T fallbackValue, string subsection, [CallerMemberName]string key = "")
        {
            var path = $".{subsection}.{key}";
            if (_proxyPaths.Contains(path))
            {
                throw new InvalidOperationException("Значение с данным ключем уже было прочитано");
            }

            var valueType = typeof(T).IsArray
                ? typeof(T).GetElementType()
                : typeof(T);
            var marshaller = GetValueMarshaller(valueType);
            var section = new Section(_config.RootSection, subsection);
            if (marshaller == null)
            {
                throw new NotSupportedException($"Тип {typeof(T)} не поддерживается.");
            }
            else if (!isKeyCorrect() || !section.IsCorrect)
            {
                throw new ArgumentException();
            }

            T readValue = fallbackValue;
            string readCommentary = null;
            ConfigKVP kvpReference = null;
            bool validKVPFound = false;
            foreach (var kvp in _config.KVPs)
            {
                if (kvp.Key == key && kvp.Section.Equals(section))
                {
                    var isParsed = marshaller.TryUnpack(kvp.Value, valueType, out dynamic parsedValue);
                    readValue = isParsed ? cast(parsedValue) : fallbackValue;
                    readCommentary = kvp.Commentary;
                    validKVPFound = isParsed;
                    if (validKVPFound)
                    {
                        kvpReference = kvp;
                        break;
                    }
                }
            }
            if (!validKVPFound)
            {
                tryAppendKVP();
            }

            _proxyPaths.Add(path);
            ConfigProxy<T> proxy = null;
            proxy = new ConfigProxy<T>(readValue, readCommentary, validKVPFound, tryUpdateValueInConfigFile, tryUpdateCommentaryInConfigFile, tryRemoveValueFromConfigFile);
            return proxy;

            ////////////////////////////////////////////////////

            void tryAppendKVP()
            {
                if (!_config.KVPs.IsReadOnly)
                {
                    kvpReference = pack(fallbackValue, null);
                    _config.KVPs.Add(kvpReference);
                }
            }
            ConfigKVP pack(T value, string commentary)
            {
                var isOk = marshaller.TryPack(value, out ConfigValue packedValue);

                if (isOk)
                {
                    commentary = commentary?.Replace(Global.NL, "");
                    return new ConfigKVP(section, key, packedValue, commentary);
                }
                else
                {
                    throw null;
                }
            }
            bool isKeyCorrect()
            {
                return key?.All(c => char.IsLetterOrDigit(c) || c == '_') ?? false;
            }
            void tryUpdateValueInConfigFile(T newValue)
            {
                var kvpIndex = _config.KVPs.Find(kvpReference).Index;
                if (!_config.KVPs.IsReadOnly && kvpIndex != -1)
                {
                    kvpReference = _config.KVPs[kvpIndex] = pack(newValue, null);
                }
            }
            void tryUpdateCommentaryInConfigFile(string newValue)
            {
                var kvpIndex = _config.KVPs.Find(kvpReference).Index;
                if (!_config.KVPs.IsReadOnly && kvpIndex != -1)
                {
                    kvpReference = _config.KVPs[kvpIndex] = pack(proxy.Value, newValue);
                }
            }
            void tryRemoveValueFromConfigFile()
            {
                var kvpIndex = _config.KVPs.Find(kvpReference).Index;
                if (!_config.KVPs.IsReadOnly && kvpIndex != -1)
                {
                    _config.KVPs.RemoveAt(kvpIndex);
                    _proxyPaths.Remove(path);
                }
            }
            T cast(dynamic value)
            {
                if (value == null)
                {
                    return value;
                }

                var valueT = value.GetType();
                var castToT = typeof(T);
                if (castToT.IsArray)
                {
                    return castAsArrayElement();
                }
                else
                {
                    return castAsSingleElement();
                }

                ///////////////////////////

                T castAsArrayElement()
                {
                    var isEmptyArray = ((Array)value).Length == 0;
                    var isArrayOfT = valueT.IsArray
                        ? (isEmptyArray ? false : value[0].GetType() == castToT.GetElementType())
                        : false;
                    if (isArrayOfT)
                    {
                        var arr = (Array)value;
                        dynamic result = Array.CreateInstance(castToT.GetElementType(), arr.Length);
                        var i = 0;
                        foreach (var item in arr)
                        {
                            result.SetValue(item, i);
                            i++;
                        }

                        return result;
                    }
                    else if (isEmptyArray)
                    {
                        dynamic result = Array.CreateInstance(castToT.GetElementType(), 0);

                        return result;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }

                T castAsSingleElement()
                {
                    var isArrayOfT = valueT.IsArray
                        ? (((Array)value).Length > 0 ? value?[0]?.GetType() == castToT : false)
                        : false;
                    if (isArrayOfT || (valueT.IsArray && value?[0] == null))
                    {
                        var arr = (Array)value;
                        if (arr.Length == 1)
                        {
                            return (T)value[0];
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                    }
                    else if (valueT == castToT)
                    {
                        return (T)value;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
            }
        }

        public ConfigProxy<T> ReadObject<T>(T fallbackValue, [CallerMemberName]string key = "")
        {
            return readObjectFrom<T>(fallbackValue, null, typeof(T), key);
        }
        public ConfigProxy<T> ReadObjectFrom<T>(T fallbackValue, string subsection, [CallerMemberName]string key = "")
        {
            return readObjectFrom<T>(fallbackValue, subsection, typeof(T), key);
        }
        ConfigProxy<T> readObjectFrom<T>(T fallbackValue, string subsection, Type valueType, [CallerMemberName]string key = "")
        {
            var path = $".{subsection}.{key}.{key}";
            if (_proxyPaths.Contains(path))
            {
                throw new InvalidOperationException("Значение с данным ключем уже было прочитано");
            }

            var marshaller = GetObjectMarshaller(valueType);
            var section = new Section(new Section(_config.RootSection, subsection), key);
            if (marshaller == null)
            {
                throw new NotSupportedException($"Тип {typeof(T)} не поддерживается.");
            }
            else if (!section.IsCorrect)
            {
                throw new ArgumentException();
            }

            T readValue = fallbackValue;
            var validKVPFound = false;
            foreach (var kvp in _config.KVPs)
            {
                if (kvp.Section.Equals(section))
                {
                    var configProxy = getProxy();
                    var isParsed = marshaller.TryUnpack(configProxy, valueType, out dynamic parsedValue);
                    readValue = isParsed ? parsedValue : fallbackValue;
                    validKVPFound = isParsed;
                    if (validKVPFound)
                    {
                        break;
                    }
                }
            }
            if (!validKVPFound)
            {
                tryAppendKVP();
            }

            _proxyPaths.Add(path);
            ConfigProxy<T> proxy = null;
            proxy = new ConfigProxy<T>(readValue, null, validKVPFound, tryUpdateValueInConfigFile, tryUpdateCommentaryInConfigFile, tryRemoveValueFromConfigFile);
            return proxy;

            ////////////////////////////////////////////////////

            void tryAppendKVP()
            {
                if (!_config.KVPs.IsReadOnly)
                {
                    pack(fallbackValue);
                }
            }
            void tryUpdateValueInConfigFile(T newValue)
            {
                if (!_config.KVPs.IsReadOnly)
                {
                    pack(newValue);
                }
            }
            void pack(T newValue)
            {
                var accessorProxy = getProxy();
                var isOk = marshaller.TryPack(accessorProxy, newValue);
                if (!isOk)
                {
                    accessorProxy.Clear();
                }
            }
            void tryUpdateCommentaryInConfigFile(string newValue)
            {
                // No notion for objects' commentaries
            }
            void tryRemoveValueFromConfigFile()
            {
                if (!_config.KVPs.IsReadOnly)
                {
                    _config.KVPs.Remove(kvp => kvp.Section.Equals(section));
                    _proxyPaths.Remove(path);
                }
            }
            ConfigAccessorProxy getProxy()
            {
                return new ConfigAccessorProxy(this, section.GetSubsection(_config.RootSection));
            }
        }

        internal ValueMarshaller GetValueMarshaller(Type valueType)
        {
            return ArrayUtils
                .ConcatSequences(_userValueMarshallers[ValueMarshallerType.EXACT], _standardValueMarshallers[ValueMarshallerType.EXACT],
                                 _userValueMarshallers[ValueMarshallerType.MULTIPLE], _standardValueMarshallers[ValueMarshallerType.MULTIPLE])
                .FirstOrDefault(m => m.IsTypeSupported(valueType));
        }
        internal ObjectMarshaller GetObjectMarshaller(Type valueType)
        {
            return ArrayUtils
                .ConcatSequences(_userObjectMarshallers, _standarsObjectMarshallers)
                .FirstOrDefault(m => m.IsTypeSupported(valueType));
        }

        public override string ToString()
        {
            return _config.ToString();
        }
    }
}
