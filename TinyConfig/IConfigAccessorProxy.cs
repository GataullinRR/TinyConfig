using System;
using System.Runtime.CompilerServices;

namespace TinyConfig
{
    public interface IConfigAccessorProxy
    {
        ConfigProxy<object> ReadValue(Type supposedType, string key);
        bool WriteValue(Type valueType, object value, string key);
        bool HasValueMarshaller(Type type);
    }
}
