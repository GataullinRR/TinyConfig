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

    //public interface IConfigAccessor
    //{
    //    ConfigSourceInfo SourceInfo { get; }
    //    ConfigAccessor Clear();
    //    ConfigAccessor Close();

    //    ConfigAccessor AddMarshaller<T>()
    //        where T : ValueMarshaller, new();
    //    ConfigProxy<T> ReadValue<T>(T fallbackValue, [CallerMemberName]string key = "");
    //    ConfigProxy<T> ReadValueFrom<T>(T fallbackValue, string subsection, [CallerMemberName]string key = "");
    //}
}
