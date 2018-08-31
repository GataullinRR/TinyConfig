using System;
using System.Collections.Generic;
using System.Reflection;
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

        public bool HasMarshaller(Type type)
        {
            return _configAccessor.GetValueMarshaller(type) != null
                || _configAccessor.GetObjectMarshaller(type) != null;
        }

        public ConfigProxy<object> ReadFrom(Type supposedType, string subsection, string key)
        {
            return readValue(supposedType, supposedType.GetDefaultValue(), subsection, key);
        }

        public bool WriteTo(Type valueType, object value, string subsection, string key)
        {
            if (_readValues.ContainsKey(key))
            {
                _readValues[key].Remove();
                _readValues.Remove(key);
            }
            return readValue(valueType, value, subsection, key).IsRead;
        }

        ConfigProxy<object> readValue(Type supposedType, object fallbackValue, string subsection, string key)
        {
            var fullSubsection = Section.ConcatSubsections(_subsection, subsection);
            var typed = typeof(ConfigAccessor)
                .GetMethod(nameof(ConfigAccessor.ReadFrom))
                .MakeGenericMethod(supposedType)
                .Invoke(_configAccessor, new object[] { fallbackValue, fullSubsection, key });
            //Do not work if T is private type defined in another assembly.
            //var value = (ConfigProxy<object>)((dynamic)typed).CastToRoot();
            var value = (ConfigProxy<object>)typed.GetType()
                .GetMethod("CastToRoot", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(typed, new object[] { });
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
