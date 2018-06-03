using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TinyConfig.Marshallers;
using Utilities;
using Utilities.Extensions;

namespace TinyConfig
{
    class DisposingIgnoreStream : Stream
    {
        readonly Stream _baseStream;

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }

        public DisposingIgnoreStream(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Read(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            
        }
    }

    class CachedConfig
    {
        readonly FileStream _file;
        readonly Encoding _encoding;
        
        public ObservableCollection<ConfigKVP> KVPs { get; }

        public CachedConfig(FileStream file, Encoding encoding)
        {
            _file = file;
            _encoding = encoding;

            KVPs = KVPExtractor.ExtractAll(new StreamReader(_file, _encoding)).ToObservable();
            KVPs.CollectionChanged += Config_CollectionChanged;
        }

        void Config_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _file.SetLength(0);
            var writer = new StreamWriter(_file, _encoding);
            writer.WriteLines(KVPs.Select(kvp => kvp.ToString()));
            writer.Flush();
            _file.Flush();
        }

        public override string ToString()
        {
            _file.Position = 0;
            return new StreamReader(_file, _encoding).ReadAllLines()
                .Aggregate((acc, line) => acc + Global.NL + line) + Global.NL;
        }
    }

    public class ConfigAccessor
    {
        readonly CachedConfig _config;
        readonly HashSet<string> _proxyKeys = new HashSet<string>();
        readonly HashSet<TypeMarshaller> _marshallers = new HashSet<TypeMarshaller>()
        {
            new BooleanMarshaller(),
            new SByteMarshaller(),
            new ByteMarshaller(),
            new Int16Marshaller(),
            new UInt16Marshaller(),
            new Int32Marshaller(),
            new UInt32Marshaller(),
            new Int64Marshaller(),
            new UInt64Marshaller(),
            new SingleMarshaller(),
            new DoubleMarshaller(),
            new StringMarshaller()
        };

        internal ConfigAccessor(CachedConfig config)
        {
            _config = config;
        }

        public ConfigAccessor AddMarshaller<T>() 
            where T : TypeMarshaller, new()
        {
            _marshallers.Add(new T());

            return this;
        }

        public ConfigProxy<T> ReadValue<T>(T fallbackValue, [CallerMemberName]string key = "")
        {
            if (_proxyKeys.Contains(key))
            {
                return new ConfigProxy<T>(fallbackValue, _ => { });
            }
            var marshaller = _marshallers.SingleOrDefault(m => m.ValueType == typeof(T));
            if (marshaller == null)
            {
                throw new NotSupportedException();
            }
            else if (!isNameCorrect())
            {
                throw new ArgumentException();
            }

            T readValue = fallbackValue;
            var kvpIndex = 0;
            bool validKVPFound = false;
            foreach (var kvp in _config.KVPs)
            {
                if (kvp.Key == key)
                {
                    var isParsed = marshaller.TryUnpack(kvp.Value, out object parsedValue);
                    readValue = isParsed ? (T)parsedValue : fallbackValue;
                    validKVPFound = isParsed;
                    if (validKVPFound)
                    {
                        break;
                    }
                }
                kvpIndex++;
            }
            if (!validKVPFound)
            {
                appendKVP();
            }

            _proxyKeys.Add(key);
            return new ConfigProxy<T>(readValue, updateKVPInConfigFile);

            void appendKVP()
            {
                var kvp = pack(fallbackValue);
                _config.KVPs.Add(kvp);
            }
            ConfigKVP pack(T value)
            {
                var isOk = marshaller.TryPack(value, out ConfigValue packedValue);

                if (isOk)
                {
                    return new ConfigKVP(key, packedValue);
                }
                else
                {
                    throw null;
                }
            }
            bool isNameCorrect()
            {
                return key?.All(c => char.IsLetterOrDigit(c) || c == '_') ?? false;
            }
            void updateKVPInConfigFile(T newValue)
            {
                _config.KVPs[kvpIndex] = pack(newValue);
            }
        }

        public override string ToString()
        {
            return _config.ToString();
        }
    }
}
