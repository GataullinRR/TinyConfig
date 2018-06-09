# TinyConfig
Библиотека для работы с конфигурационными файлами. Работает под .NET 4.5.2

# Пример:
```CSharp
static readonly ConfigAccessor _config = Configurable.CreateConfig("ExampleConfigFile");
static readonly ConfigProxy<int> SOME_INT = _config.ReadValue(100);
static readonly ConfigProxy<string> SOME_MULTILINE_STRING_INT = _config.ReadValue("'Hello' \n\r all!");

static void Main(string[] args)
{
    Console.WriteLine($"SOME_INT: {SOME_INT}");
    Console.WriteLine($"SOME_MULTILINE_STRING_INT: {SOME_MULTILINE_STRING_INT}");
    SOME_MULTILINE_STRING_INT.Value = "New value";
    Console.WriteLine($"SOME_MULTILINE_STRING_INT(changed): {SOME_MULTILINE_STRING_INT}");

    Console.ReadKey();
}
```   
Консольный вывод:

    SOME_INT: 100
    SOME_MULTILINE_STRING_INT: 'Hello'
     all!
    SOME_MULTILINE_STRING_INT(changed): New value

Сгенерированный файл:

    SOME_INT =100
    SOME_MULTILINE_STRING_INT = #'New value'
