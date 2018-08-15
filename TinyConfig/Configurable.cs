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
    public enum ConfigAccess
    {
        READ_ONLY,
        READ_WRITE
    }

    public static class Configurable
    {
        const string CONFIG_NAME_TEMPLATE = "{0}.ini";
        const ConfigAccess DEFAULT_CONFIG_ACCESS = ConfigAccess.READ_WRITE;
        static readonly Encoding DEFAUL_ENCODING = Encoding.UTF8;

        static readonly HashSet<ConfigStorageProxy> _openedFiles = new HashSet<ConfigStorageProxy>();

        public static string BaseDirectory { get; }

        static Configurable()
        {
            var root = new Uri(Path.GetDirectoryName(Assembly.GetCallingAssembly().CodeBase));
            BaseDirectory = Path.Combine(root.LocalPath, "Settings");
            if (!IOUtils.TryCreateDirectoryIfNotExist(BaseDirectory))
            {
                throw null;
            }
        }

        public static IConfigAccessor CreateConfig(Type owner)
        {
            return CreateConfig(owner.FullName);
        }
        public static IConfigAccessor CreateConfig(string configFileName)
        {
            return CreateConfig(configFileName, "");
        }
        public static IConfigAccessor CreateConfig(string configFileName, string relativeDirPath)
        {
            return CreateConfig(configFileName, relativeDirPath, DEFAUL_ENCODING);
        }
        public static IConfigAccessor CreateConfig(string configFileName, string relativeDirPath, string section)
        {
            return CreateConfig(configFileName, relativeDirPath, DEFAUL_ENCODING, DEFAULT_CONFIG_ACCESS, section);
        }
        public static IConfigAccessor CreateConfig(string configFileName, string relativeDirPath, Encoding encoding)
        {
            return CreateConfig(configFileName, relativeDirPath, encoding, DEFAULT_CONFIG_ACCESS);
        }
        public static IConfigAccessor CreateConfig(string configFileName, string relativeDirPath, ConfigAccess access)
        {
            return CreateConfig(configFileName, relativeDirPath, DEFAUL_ENCODING, access);
        }
        public static IConfigAccessor CreateConfig
            (string configFileName, string relativeDirPath, Encoding encoding, ConfigAccess access)
        {
            return CreateConfig(configFileName, relativeDirPath, encoding, access, null);
        }
        public static IConfigAccessor CreateConfig
            (string configFileName, string relativeDirPath, Encoding encoding, ConfigAccess access, string section)
        {
            var configPath = Path
                .Combine(BaseDirectory, relativeDirPath, CONFIG_NAME_TEMPLATE.Format(configFileName as object));
            var config = _openedFiles.SingleOrDefault(c => c.FilePath == configPath);
            if (config == null)
            {
                FileStream configFile = IOUtils.TryCreateFileIfNotExistOrOpenOrNull(configPath);
                if (configFile == null)
                {
                    throw new Exception("Не удается получить доступ к файлу.");
                }
                else
                {
                    config = new ConfigStorageProxy(configFile);
                    _openedFiles.Add(config);
                }
            }
            var stream = config.GetNewStream(access, section);

            return new ConfigAccessor(
                new ConfigReaderWriter(stream, encoding, section), 
                new ConfigSourceInfo(true, configPath));
        }

        public static IConfigAccessor CreateConfig(Stream configStream)
        {
            return CreateConfig(configStream, null, DEFAUL_ENCODING);
        }
        public static IConfigAccessor CreateConfig(Stream configStream, string section, Encoding encoding)
        {
            var config = new ConfigStorageProxy(configStream);
            _openedFiles.Add(config);

            var stream = config.GetNewStream(DEFAULT_CONFIG_ACCESS, section);

            return new ConfigAccessor(
                new ConfigReaderWriter(stream, encoding, section),
                new ConfigSourceInfo(false, null));
        }

        public static void ReleaseFile(string configFilePath)
        {
            if (configFilePath == null)
            {
                throw new ArgumentNullException();
            }

            var file = _openedFiles.SingleOrDefault(f => f.FilePath == configFilePath);
            if (file != null)
            {
                file.Dispose();
                _openedFiles.Remove(file);
            }
        }
    }
}
