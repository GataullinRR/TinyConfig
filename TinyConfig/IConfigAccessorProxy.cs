using System;
using System.Runtime.CompilerServices;

namespace TinyConfig
{
    public interface IConfigAccessorProxy
    {
        ConfigProxy<object> ReadFrom(Type supposedType, string subsection, string key);
        bool WriteTo(Type valueType, object value, string subsection, string key);
        bool HasMarshaller(Type type);
    }
}
