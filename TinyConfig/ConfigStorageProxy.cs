using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig
{
    /// <summary>
    /// Предоставляет потоки для взаимодействия с отдельными секциями файла с заданным уровнем доступа
    /// </summary>
    class ConfigStorageProxy : Disposable
    {
        /// <summary>
        /// Разбивает один поток к конфигу на несколько независимых потоков к конкретным секциям этого конфига
        /// </summary>
        class StreamAggregator : Disposable
        {
            class NotifiableStream : StreamBase
            {
                public class StreamModifiedEventArgs : EventArgs
                {

                }

                public event EventHandler<StreamModifiedEventArgs> Modified = delegate { };
                public event EventHandler<EventArgs> Closed = delegate { };

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

                public override void Close()
                {
                    base.Close();
                    Closed.Invoke(this, EventArgs.Empty);
                }
            }

            class StreamInfo
            {
                public string SectionName { get; }
                public NotifiableStream Stream { get; }
                public int PositionInBase { get; }

                public StreamInfo(string sectionName, NotifiableStream stream, int positionInBase)
                {
                    SectionName = sectionName;
                    Stream = stream;
                    PositionInBase = positionInBase;
                }
            }

            readonly Stream _baseStream;
            bool _isBaseStreamClosed;
            readonly SectionsFinder.SectionInfo[] _sections;
            readonly List<StreamInfo> _sectionStreams = new List<StreamInfo>();

            public StreamAggregator(Stream baseStream)
            {
                _baseStream = baseStream;

                var lines = new StreamReader(_baseStream).ReadAllLines().ToArray();
                var sections = SectionsFinder.GetSections(lines);
                _sections = sections.ToArray();
            }

            public Stream CreateStream(string sectionName)
            {
                throwIfDisposed();

                var sectionIndex = _sections.Find(s => s.Section.FullName == sectionName);
                var section = sectionIndex >= 0 ? _sections[sectionIndex] : null;

                if (_sectionStreams.Any(si => si.SectionName == sectionName))
                {
                    throw new InvalidOperationException();
                }

                NotifiableStream stream = null;
                if (section == null)
                {
                    stream = new NotifiableStream(new MemoryStream());
                    sectionIndex = Math.Max(_sections.Length - 1, _sectionStreams.EmptyToNull()?.Max(s => s.PositionInBase) ?? 0);
                }
                else
                {
                    stream = new NotifiableStream(readSection());
                }
                hookEvents(stream);
                var streamInfo = new StreamInfo(sectionName, stream, sectionIndex);
                _sectionStreams.Add(streamInfo);

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
                throwIfDisposed();

                return _sectionStreams.FirstOrDefault(si => si.SectionName == sectionName)?.Stream;
            }

            void closeBaseStream(object sender, EventArgs e)
            {
                var stream = (NotifiableStream)sender;
                unhookEvents(stream);
                if (_sectionStreams.Count == 1)
                {
                    _baseStream.Flush();
                    _baseStream.Close();
                    _isBaseStreamClosed = true;
                }
                else
                {
                    _sectionStreams.Remove(s => s.Stream == stream);
                }
            }

            void hookEvents(NotifiableStream stream)
            {
                stream.Modified += updateBaseStream;
                stream.Closed += closeBaseStream;
            }
            void unhookEvents(NotifiableStream stream)
            {
                stream.Modified -= updateBaseStream;
                stream.Closed -= closeBaseStream;
            }

            void updateBaseStream(object sender, NotifiableStream.StreamModifiedEventArgs e)
            {
                _baseStream.SetLength(0);
                foreach (var stream in _sectionStreams.OrderBy(s => s.PositionInBase))
                {
                    var oldPosition = stream.Stream.Position;
                    stream.Stream.Position = 0;
                    _baseStream.Write(stream.Stream.ReadToEnd());
                    stream.Stream.Position = oldPosition;
                }
                _baseStream.Flush();
            }

            protected override void disposeManagedState()
            {
                foreach (var stream in _sectionStreams)
                {
                    unhookEvents(stream.Stream);
                    stream.Stream.Dispose();
                }
                _baseStream.Dispose();
            }
        }

        /// <summary>
        /// Обертка над потоком, которая может ограничивать доступ на запись
        /// </summary>
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

        protected override void disposeManagedState()
        {
            _streamAggregator.Dispose();
        }

        public override int GetHashCode()
        {
            return new { FilePath, _fileStorage }.GetHashCode();
        }
    }
}
