using MVVMUtilities.Types;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig
{
    class ConfigReaderWriter
    {
        readonly Stream _file;
        readonly Encoding _encoding;

        public Section RootSection { get; }
        public EnhancedObservableCollection<ConfigKVP> KVPs { get; }

        public ConfigReaderWriter(Stream file, Encoding encoding, string rootSection)
        {
            _file = file;
            _encoding = encoding;
            RootSection = new Section(rootSection);

            KVPs = new EnhancedObservableCollection<ConfigKVP>(
                KVPExtractor.ExtractAll(new StreamReader(_file, _encoding)));
            KVPs.CollectionChanged += Config_CollectionChanged;
            if (!file.CanWrite)
            {
                KVPs.MakeReadOnly();
            }
        }

        void Config_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var sections = from g in KVPs.Group(v => v.Section, (a, b) => a.Equals(b))
                           orderby g.Key.Order
                           orderby g.Key.FullName
                           select new { Section = g.Key, Body = g.ToArray() };

            _file.SetLength(0);
            var writer = new StreamWriter(_file, _encoding);
            foreach (var group in sections)
            {
                if (!group.Section.IsInsideSection(RootSection.FullName))
                {
                    throw new InvalidOperationException();
                }

                if (group.Section != null && !group.Section.IsRoot)
                {
                    writer.WriteLine(group.Section.ToString());
                }
                foreach (var kvp in group.Body)
                {
                    writer.WriteLine(kvp.ToString());
                }
            }
            writer.Flush();
            _file.Flush();
        }

        public void Close()
        {
            _file.Close();
            KVPs.MakeReadOnly();
        }

        public override string ToString()
        {
            var EMPTY = "" + Global.NL;

            _file.Position = 0;
            if (_file.Length == 0)
            {
                return EMPTY;
            }
            else
            {
                var lines = new StreamReader(_file, _encoding).ReadAllLines().ToArray();
                return lines.IsEmpty() 
                    ? EMPTY
                    : (lines.Aggregate((acc, line) => acc + Global.NL + line) + Global.NL);
            }
        }
    }
}
