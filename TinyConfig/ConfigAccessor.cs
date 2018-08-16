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
    public class Proxy : IConfigAccessorProxy
    {
        readonly ConfigAccessor _configAccessor;

        public Proxy(ConfigAccessor configAccessor)
        {

        }

        public bool HasValueMarshaller(Type type)
        {
            return _configAccessor.GetValueMarshaller(type) != null;
        }

        public ConfigProxy<T> ReadValue<T>(string key)
        {
            var value = _configAccessor.ReadValue(default(T), key);
            return value.IsRead ? value : null;
        }

        public ConfigProxy<object> ReadValue(Type supposedType, string key)
        {
            throw new NotImplementedException();
        }

        public ConfigProxy<T> ReadValueFrom<T>(string subsection, string key)
        {
            throw new NotImplementedException();
        }

        public bool WriteValue<T>(T value, string key)
        {
            throw new NotImplementedException();
        }

        public bool WriteValueFrom<T>(T value, string subsection, string key)
        {
            throw new NotImplementedException();
        }
    }

    public class ConfigAccessor : IConfigAccessor
    {
        enum ValueMarshallerType
        {
            EXACT,
            MULTIPLE,
        }

        readonly ConfigReaderWriter _config;
        readonly HashSet<string> _proxyPaths = new HashSet<string>();
        readonly Dictionary<ValueMarshallerType, List<ValueMarshaller>> _valueMarshallers
            = new Dictionary<ValueMarshallerType, List<ValueMarshaller>>()
            {
                {
                    ValueMarshallerType.EXACT,
                    new List<ValueMarshaller>()
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
                        new MemoryStreamMarshaller(),
                    }
                },
                {
                    ValueMarshallerType.MULTIPLE,
                    new List<ValueMarshaller>()
                    {
                        new EnumMarshaller()
                    }
                }
            };
        readonly List<ObjectMarshaller> _objectMarshallers = new List<ObjectMarshaller>()
        {
             new FlatStructObjectMarshaller()
        };

        public ConfigSourceInfo SourceInfo { get; }

        internal ConfigAccessor(ConfigReaderWriter config, ConfigSourceInfo configSourceInfo)
        {
            _config = config;
            SourceInfo = configSourceInfo;
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
            if (typeof(ExactTypeMarshaller).IsAssignableFrom(typeof(T)))
            {
                _valueMarshallers[ValueMarshallerType.EXACT].Insert(0, new T());
            }
            else
            {
                _valueMarshallers[ValueMarshallerType.MULTIPLE].Insert(0, new T());
            }

            return this;
        }
        public ConfigAccessor AddObjectMarshaller<T>()
            where T : ObjectMarshaller, new()
        {
            _objectMarshallers.Insert(0, new T());

            return this;
        }

        public ConfigProxy<T> ReadValue<T>(T fallbackValue, [CallerMemberName]string key = "")
        {
            return ReadValueFrom(fallbackValue, null, key);
        }
        public ConfigProxy<T> ReadValueFrom<T>(T fallbackValue, string subsection, [CallerMemberName]string key = "")
        {
            var d = (ConfigProxy<object>)ReadValue("");
        }
        ConfigProxy<T> readValueFrom<T>(T fallbackValue, string subsection, [CallerMemberName]string key = "")
        {
            var path = $".{subsection}.{key}";
            if (_proxyPaths.Contains(path))
            {
                throw new InvalidOperationException("Значение с данным ключем уже было прочитано");
            }

            var valueType = typeof(T).IsArray
                ? typeof(T).GetElementType()
                : typeof(T);
            var marshaller = ArrayUtils
                .ConcatSequences(_valueMarshallers[ValueMarshallerType.EXACT], _valueMarshallers[ValueMarshallerType.MULTIPLE])
                .FirstOrDefault(m => m.IsTypeSupported(valueType));
            var section = new Section(_config.RootSection, subsection);
            if (marshaller == null)
            {
                throw new NotSupportedException();
            }
            else if (!isKeyCorrect() || !section.IsCorrect)
            {
                throw new ArgumentException();
            }

            T readValue = fallbackValue;
            string readCommentary = null;
            var kvpIndex = 0;
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
                        break;
                    }
                }
                kvpIndex++;
            }
            if (!validKVPFound)
            {
                tryAppendKVP();
            }

            _proxyPaths.Add(path);
            ConfigProxy<T> proxy = null;
            proxy = new ConfigProxy<T>(readValue, readCommentary, validKVPFound, tryUpdateValueInConfigFile, tryUpdateCommentaryInConfigFile);
            return proxy;

            ////////////////////////////////////////////////////

            void tryAppendKVP()
            {
                if (!_config.KVPs.IsReadOnly)
                {
                    var kvp = pack(fallbackValue, null);
                    _config.KVPs.Add(kvp);
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
                if (!_config.KVPs.IsReadOnly)
                {
                    _config.KVPs[kvpIndex] = pack(newValue, null);
                }
            }
            void tryUpdateCommentaryInConfigFile(string newValue)
            {
                if (!_config.KVPs.IsReadOnly)
                {
                    _config.KVPs[kvpIndex] = pack(proxy.Value, newValue);
                }
            }
            T cast(dynamic value)
            {
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
                        ? (((Array)value).Length > 0 ? value[0].GetType() == castToT : false)
                        : false;
                    if (isArrayOfT)
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

        ConfigProxy<T> readObjectFrom<T>(T fallbackValue, string subsection, Type type, [CallerMemberName]string key = "")
        {
            var path = $".{subsection}.{key}.{key}";
            if (_proxyPaths.Contains(path))
            {
                throw new InvalidOperationException("Значение с данным ключем уже было прочитано");
            }

            var valueType = type.IsArray
                ? type.GetElementType()
                : type;
            var marshaller = _objectMarshallers.FirstOrDefault(m => m.IsTypeSupported(valueType));
            var section = new Section(new Section(_config.RootSection, subsection), key);
            if (marshaller == null)
            {
                throw new NotSupportedException();
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
                    var isParsed = marshaller.TryUnpack(getProxy(), valueType, out dynamic parsedValue);
                    readValue = isParsed ? cast(parsedValue) : fallbackValue;
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
            proxy = new ConfigProxy<T>(readValue, null, validKVPFound, tryUpdateValueInConfigFile, tryUpdateCommentaryInConfigFile);
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
            T cast(dynamic value)
            {
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
                        ? (((Array)value).Length > 0 ? value[0].GetType() == castToT : false)
                        : false;
                    if (isArrayOfT)
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

        IConfigAccessorProxy getProxy()
        {

        }

        internal ValueMarshaller GetValueMarshaller(Type t)
        {

        }

        public override string ToString()
        {
            return _config.ToString();
        }
    }
}
