using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;

namespace TinyConfig
{
    public static class Configurable
    {
        const string CONFIG_NAME_TEMPLATE = "{0}.ini";

        public static readonly HashSet<string> _openedFiles = new HashSet<string>();

        public static string  BaseDirectory { get; }

        static Configurable()
        {
            var root = new Uri(Path.GetDirectoryName(Assembly.GetCallingAssembly().CodeBase));
            BaseDirectory = Path.Combine(root.LocalPath, "Settings");
            if (!IOUtils.TryCreateDirectoryIfNotExist(BaseDirectory))
            {
                throw null;
            }
        }

        public static ConfigAccessor CreateConfig(Type owner)
        {
            return CreateConfig(owner.FullName);
        }
        public static ConfigAccessor CreateConfig(string configFileName)
        {
            return CreateConfig(configFileName, "", Encoding.UTF8);
        }
        public static ConfigAccessor CreateConfig(string configFileName, string relativeDirPath)
        {
            return CreateConfig(configFileName, relativeDirPath, Encoding.UTF8);
        }
        public static ConfigAccessor CreateConfig(string configFileName, string relativeDirPath, Encoding encoding)
        {
            var configPath = Path
                .Combine(BaseDirectory, relativeDirPath, CONFIG_NAME_TEMPLATE.Format(configFileName as object));
            if (_openedFiles.Contains(configPath))
            {
                throw new Exception("Данный конфигурационный файл уже используется.");
            }

            FileStream config = IOUtils.TryCreateFileIfNotExistOrOpenOrNull(configPath);
            if (config == null)
            {
                throw new Exception("Не удается получить доступ к файлу.");
            }
            else
            {
                _openedFiles.Add(configPath);
            }

            return new ConfigAccessor(new CachedConfig(config, encoding));
        }
    }
}
