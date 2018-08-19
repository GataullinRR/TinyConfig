using System;
using System.Collections.Generic;
using Utilities.Extensions;

namespace TinyConfig
{
    class ConfigAccessorProxy : IConfigAccessorProxy
    {
        readonly ConfigAccessor _configAccessor;
        readonly string _subsection;
        readonly Dictionary<string, ConfigProxy<object>> _readValues = new Dictionary<string, ConfigProxy<object>>();

        public ConfigAccessorProxy(ConfigAccessor configAccessor, string subsection)
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
}
