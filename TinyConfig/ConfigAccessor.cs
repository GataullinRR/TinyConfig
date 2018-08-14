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
        enum MarshallerType
        {
            EXACT,
            MULTIPLE,
        }

        readonly ConfigReaderWriter _config;
        readonly HashSet<string> _proxyPaths = new HashSet<string>();
        readonly Dictionary<MarshallerType, List<TypeMarshaller>> _marshallers
            = new Dictionary<MarshallerType, List<TypeMarshaller>>()
            {
                {
                    MarshallerType.EXACT,
                    new List<TypeMarshaller>()
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
                    MarshallerType.MULTIPLE,
                    new List<TypeMarshaller>()
                    {
                        new EnumMarshaller()
                    }
                }
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
            where T : TypeMarshaller, new()
        {
            if (typeof(ExactTypeMarshaller).IsAssignableFrom(typeof(T)))
            {
                _marshallers[MarshallerType.EXACT].Insert(0, new T());
            }
            else
            {
                _marshallers[MarshallerType.MULTIPLE].Insert(0, new T());
            }

            return this;
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
            var marshaller = ArrayUtils
                .ConcatSequences(_marshallers[MarshallerType.EXACT], _marshallers[MarshallerType.MULTIPLE])
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
            proxy = new ConfigProxy<T>(readValue, readCommentary, tryUpdateValueInConfigFile, tryUpdateCommentaryInConfigFile);
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

        public override string ToString()
        {
            return _config.ToString();
        }
    }
}
