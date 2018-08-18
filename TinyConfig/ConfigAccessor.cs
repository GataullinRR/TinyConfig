using System;
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
        readonly string _subsection;
        readonly Dictionary<string, ConfigProxy<object>> _readValues = new Dictionary<string, ConfigProxy<object>>();

        public Proxy(ConfigAccessor configAccessor, string subsection)
        {
            _configAccessor = configAccessor;
            _subsection = subsection;
        }

        public bool HasValueMarshaller(Type type)
        {
            return _configAccessor.GetValueMarshaller(type) != null;
        }

        public ConfigProxy<object> ReadValue(Type supposedType, string key)
        {
            return readValue(supposedType, supposedType.GetDefaultValue(), key);
        }

        public bool WriteValue(Type valueType, object value, string key)
        {
            if (_readValues.ContainsKey(key))
            {
                _readValues[key].Remove();
                _readValues.Remove(key);
            }
            return readValue(valueType, value, key).IsRead;
        }

        ConfigProxy<object> readValue(Type supposedType, object fallbackValue, string key)
        {
            var typed = typeof(ConfigAccessor)
                .GetMethod(nameof(ConfigAccessor.ReadValueFrom))
                .MakeGenericMethod(supposedType)
                .Invoke(_configAccessor, new object[] { fallbackValue, _subsection, key });
            var value = (ConfigProxy<object>)((dynamic)typed).CastToRoot();
            _readValues.Add(key, value);

            return value;
        }

        public void Clear()
        {
            _readValues.ForEach(kvp => kvp.Value.Remove());
            _readValues.Clear();
        }
    }

    public class ConfigAccessor
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
            if (typeof(ExactValueMarshaller).IsAssignableFrom(typeof(T)))
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
            proxy = new ConfigProxy<T>(readValue, readCommentary, validKVPFound, tryUpdateValueInConfigFile, tryUpdateCommentaryInConfigFile, tryRemoveValueFromConfigFile);
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
            void tryRemoveValueFromConfigFile()
            {
                if (!_config.KVPs.IsReadOnly)
                {
                    _config.KVPs.RemoveAt(kvpIndex);
                    _proxyPaths.Remove(path);
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

        public ConfigProxy<T> ReadObject<T>(T fallbackValue, [CallerMemberName]string key = "")
        {
            return ReadObjectFrom<T>(fallbackValue, null, typeof(T), key);
        }
        public ConfigProxy<T> ReadObjectFrom<T>(T fallbackValue, string subsection, [CallerMemberName]string key = "")
        {
            return ReadObjectFrom<T>(fallbackValue, subsection, typeof(T), key);
        }
        public ConfigProxy<T> ReadObjectFrom<T>(T fallbackValue, string subsection, Type valueType, [CallerMemberName]string key = "")
        {
            var path = $".{subsection}.{key}.{key}";
            if (_proxyPaths.Contains(path))
            {
                throw new InvalidOperationException("Значение с данным ключем уже было прочитано");
            }

            var marshaller = _objectMarshallers.FirstOrDefault(m => m.IsTypeSupported(valueType));
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
            Proxy getProxy()
            {
                return new Proxy(this, section.GetSubsection(_config.RootSection));
            }
        }

        internal ValueMarshaller GetValueMarshaller(Type valueType)
        {
            return ArrayUtils
                .ConcatSequences(_valueMarshallers[ValueMarshallerType.EXACT], _valueMarshallers[ValueMarshallerType.MULTIPLE])
                .FirstOrDefault(m => m.IsTypeSupported(valueType));
        }

        public override string ToString()
        {
            return _config.ToString();
        }
    }
}
