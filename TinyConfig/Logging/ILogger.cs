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
    enum LogLevels
    {
        INFO,
        ERROR,
    }

    interface ILogger
    {
        void Info(string message, [CallerMemberName]string methodName = "");
        void Error(string message, [CallerMemberName]string methodName = "");

        void Log(string message, LogLevels level, [CallerMemberName]string methodName = "");
    }
}
