using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyConfig;

namespace Example
{
    class Program
    {
        static readonly IConfigAccessor _config = Configurable.CreateConfig("ExampleConfigFile");
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
    }
}
