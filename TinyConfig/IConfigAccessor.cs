using System;
using System.Runtime.CompilerServices;

namespace TinyConfig
{
    public interface IConfigAccessorProxy
    {
        ConfigProxy<T> ReadValue<T>(string key);
        ConfigProxy<object> ReadValue(Type supposedType, string key);
        ConfigProxy<T> ReadValueFrom<T>(string subsection, string key);
        bool WriteValue<T>(T value, string key);
        bool WriteValueFrom<T>(T value, string subsection, string key);
        bool HasValueMarshaller(Type type);
    }

    public interface IConfigAccessor
    {
        ConfigSourceInfo SourceInfo { get; }
        ConfigAccessor Clear();
        ConfigAccessor Close();

        ConfigAccessor AddMarshaller<T>()
            where T : ValueMarshaller, new();
        ConfigProxy<T> ReadValue<T>(T fallbackValue, [CallerMemberName]string key = "");
        ConfigProxy<T> ReadValueFrom<T>(T fallbackValue, string subsection, [CallerMemberName]string key = "");
    }
}
