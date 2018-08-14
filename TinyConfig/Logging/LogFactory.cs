using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;

namespace TinyConfig
{
    static class LogFactory
    {
        class A : ILogger
        {
            public void Error(string message, [CallerMemberName] string methodName = "")
                => Log(message, LogLevels.ERROR, methodName);
            public void Info(string message, [CallerMemberName] string methodName = "")
                => Log(message, LogLevels.INFO, methodName);

            public void Log(string message, LogLevels level, [CallerMemberName] string methodName = "")
            {
                return;
            }
        }

        public static ILogger GetLogger()
        {
            return new A();
        }
    }
}
