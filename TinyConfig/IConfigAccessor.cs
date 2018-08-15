using System;
using System.Runtime.CompilerServices;

namespace TinyConfig
{
    public interface IConfigAccessorProxy
    {
        ConfigProxy<T> ReadValue<T>([CallerMemberName]string key = "");
        ConfigProxy<object> ReadValue(Type supposedType, [CallerMemberName]string key = "");
        ConfigProxy<T> ReadValueFrom<T>(string subsection, [CallerMemberName]string key = "");
        bool WriteValue<T>(T value, [CallerMemberName]string key = "");
        bool WriteValueFrom<T>(T value, string subsection, [CallerMemberName]string key = "");
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
