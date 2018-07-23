using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig
{
    public enum ConfigAccess
    {
        READ_ONLY,
        READ_WRITE
    }

    class ConfigStorageProxy
    {
        class StreamProxy : Stream
        {
            readonly Stream _stream;

            public override bool CanRead => _stream.CanRead;

            public override bool CanSeek => _stream.CanSeek;

            public override bool CanWrite { get; }

            public override long Length => _stream.Length;

            public override long Position { get; set; }

            public StreamProxy(Stream stream, bool canWrite)
            {
                _stream = stream;
                Position = _stream.Position;
                CanWrite = canWrite;
            }

            public override void Flush()
            {
                _stream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                _stream.Position = Position;
                var result = _stream.Read(buffer, offset, count);
                Position = _stream.Position;

                return result;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                _stream.Position = Position;
                var result = _stream.Seek(offset, origin);
                Position = _stream.Position;

                return result;
            }

            public override void SetLength(long value)
            {
                _stream.Position = Position;
                _stream.SetLength(value);
                Position = _stream.Position;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (!CanWrite)
                {
                    throw new NotSupportedException();
                }

                _stream.Position = Position;
                _stream.Write(buffer, offset, count);
                Position = _stream.Position;
            }
        }

        class StreamAggregator
        {
            class NotifiableStream : StreamBase
            {
                public class StreamModifiedEventArgs : EventArgs
                {

                }

                public event EventHandler<StreamModifiedEventArgs> Modified = delegate { };

                public NotifiableStream(Stream baseStream)
                    : base(baseStream)
                {

                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    base.Write(buffer, offset, count);
                    Modified.Invoke(this, new StreamModifiedEventArgs());
                }

                public override void SetLength(long value)
                {
                    base.SetLength(value);
                    Modified.Invoke(this, new StreamModifiedEventArgs());
                }
            }

            class StreamInfo
            {
                public string SectionName { get; }
                public Stream Stream { get; }
                public int PositionInBase { get; }

                public StreamInfo(string sectionName, Stream stream, int positionInBase)
                {
                    SectionName = sectionName;
                    Stream = stream;
                    PositionInBase = positionInBase;
                }
            }

            readonly Stream _baseStream;
            readonly SectionsFinder.SectionInfo[] _sections;
            readonly List<StreamInfo> _streams = new List<StreamInfo>();

            public StreamAggregator(Stream baseStream)
            {
                _baseStream = baseStream;

                var lines = new StreamReader(_baseStream).ReadAllLines().ToArray();
                var sections = SectionsFinder.GetSections(lines);
                _sections = sections.ToArray();
            }

            public Stream CreateStream(string sectionName)
            {
                var sectionIndex = _sections.Find(s => s.Section.FullName == sectionName);
                var section = sectionIndex >= 0 ? _sections[sectionIndex] : null;

                if (_streams.Any(si => si.SectionName == sectionName))
                {
                    throw new InvalidOperationException();
                }

                NotifiableStream stream = null;
                if (section == null)
                {
                    stream = new NotifiableStream(new MemoryStream());
                    sectionIndex = Math.Max(_sections.Length - 1, _streams.EmptyToNull()?.Max(s => s.PositionInBase) ?? 0);
                }
                else
                {
                    stream = new NotifiableStream(readSection());
                }
                stream.Modified += updateBaseStream;
                var streamInfo = new StreamInfo(sectionName, stream, sectionIndex);
                _streams.Add(streamInfo);

                return stream;

                Stream readSection()
                {
                    var ms = new MemoryStream();
                    var sw = new StreamWriter(ms);
                    sw.WriteLines(section.FullSection);
                    sw.Flush();
                    ms.Position = 0;

                    return ms;
                }
            }

            public Stream TryGetCreatedStream(string sectionName)
            {
                return _streams.FirstOrDefault(si => si.SectionName == sectionName)?.Stream;
            }

            void updateBaseStream(object sender, NotifiableStream.StreamModifiedEventArgs e)
            {
                _baseStream.SetLength(0);
                foreach (var stream in _streams.OrderBy(s => s.PositionInBase))
                {
                    var oldPosition = stream.Stream.Position;
                    stream.Stream.Position = 0;
                    _baseStream.Write(stream.Stream.ReadToEnd());
                    stream.Stream.Position = oldPosition;
                }
            }
        }

        class SectionAccessInfo
        {
            public Section Section { get; }
            public bool ReadWriteStreamCreated { get; set; }
            public bool AnyStreamCreated { get; set; }

            public SectionAccessInfo(Section section, bool readWriteStreamCreated, bool anyStreamCreated)
            {
                Section = section ?? throw new ArgumentNullException(nameof(section));
                ReadWriteStreamCreated = readWriteStreamCreated;
                AnyStreamCreated = anyStreamCreated;
            }
        }

        readonly Stream _fileStorage;
        readonly StreamAggregator _streamAggregator;
        readonly List<SectionAccessInfo> _sectionInfos = new List<SectionAccessInfo>();

        public string FilePath { get; }

        public ConfigStorageProxy(FileStream fileStorage)
            : this((Stream)fileStorage)
        {
            FilePath = fileStorage.Name;
        }
        public ConfigStorageProxy(Stream fileStorage)
        {
            _fileStorage = fileStorage;
            _streamAggregator = new StreamAggregator(_fileStorage);
        }

        public Stream GetNewStream(ConfigAccess access, string section)
        {
            var stream = _streamAggregator.TryGetCreatedStream(section) 
                      ?? _streamAggregator.CreateStream(section);
            {
                var thisSectionInfo = _sectionInfos.SingleOrDefault(si => si.Section.FullName == section);
                if (thisSectionInfo == null)
                {
                    _sectionInfos.Add(new SectionAccessInfo(new Section(section), false, false));
                }
            }
            var sectionInfos = _sectionInfos.Where(si => si.Section.IsInsideSection(section));
            var readWriteStreamCreated = sectionInfos.Any(si => si.ReadWriteStreamCreated);
            var anyStreamCreated = sectionInfos.Any(si => si.AnyStreamCreated);
            switch (access)
            {
                case ConfigAccess.READ_ONLY:
                    if (readWriteStreamCreated)
                    {
                        throw new InvalidOperationException("Не разрешается создавать поток с доступом на чтение, если поток с доступом на четение/запись уже был создан.");
                    }
                    else
                    {
                        foreach (var si in sectionInfos)
                        {
                            si.AnyStreamCreated = true;
                        }
                        return new StreamProxy(stream, false);
                    }
                case ConfigAccess.READ_WRITE:
                    if (readWriteStreamCreated)
                    {
                        throw new InvalidOperationException("Поток с доступом на четение/запись уже был создан.");
                    }
                    else if (anyStreamCreated)
                    {
                        throw new InvalidOperationException("Не разрешается создавать поток с доступом на четение/запись, если поток с любым уровнем доступа уже был создан.");
                    }
                    else
                    {
                        foreach (var si in sectionInfos)
                        {
                            si.ReadWriteStreamCreated = true;
                            si.AnyStreamCreated = true;
                        }
                        return new StreamProxy(stream, true);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        public override int GetHashCode()
        {
            return new { FilePath, _fileStorage }.GetHashCode();
        }
    }

    public static class Configurable
    {
        const string CONFIG_NAME_TEMPLATE = "{0}.ini";
        const ConfigAccess DEFAULT_CONFIG_ACCESS = ConfigAccess.READ_WRITE;
        static readonly Encoding DEFAUL_ENCODING = Encoding.UTF8;

        static readonly HashSet<ConfigStorageProxy> _openedFiles = new HashSet<ConfigStorageProxy>();

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
            return CreateConfig(configFileName, "");
        }
        public static ConfigAccessor CreateConfig(string configFileName, string relativeDirPath)
        {
            return CreateConfig(configFileName, relativeDirPath, DEFAUL_ENCODING);
        }
        public static ConfigAccessor CreateConfig(string configFileName, string relativeDirPath, string section)
        {
            return CreateConfig(configFileName, relativeDirPath, DEFAUL_ENCODING, DEFAULT_CONFIG_ACCESS, section);
        }
        public static ConfigAccessor CreateConfig(string configFileName, string relativeDirPath, Encoding encoding)
        {
            return CreateConfig(configFileName, relativeDirPath, encoding, DEFAULT_CONFIG_ACCESS);
        }
        public static ConfigAccessor CreateConfig(string configFileName, string relativeDirPath, ConfigAccess access)
        {
            return CreateConfig(configFileName, relativeDirPath, DEFAUL_ENCODING, access);
        }
        public static ConfigAccessor CreateConfig
            (string configFileName, string relativeDirPath, Encoding encoding, ConfigAccess access)
        {
            return CreateConfig(configFileName, relativeDirPath, encoding, access, null);
        }
        public static ConfigAccessor CreateConfig
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

            return new ConfigAccessor(new ConfigReaderWriter(stream, encoding, section));
        }

        public static ConfigAccessor CreateConfig(Stream configStream)
        {
            return CreateConfig(configStream, null, DEFAUL_ENCODING);
        }
        public static ConfigAccessor CreateConfig(Stream configStream, string section, Encoding encoding)
        {
            var config = new ConfigStorageProxy(configStream);
            _openedFiles.Add(config);

            var stream = config.GetNewStream(DEFAULT_CONFIG_ACCESS, section);

            return new ConfigAccessor(new ConfigReaderWriter(stream, encoding, section));
        }
    }
}
