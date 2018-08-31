using MVVMUtilities.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig
{
    abstract class ConfigEntity
    {
        public enum Types
        {
            SECTION,
            KVP,
        }

        public Types Type { get; }

        public ConfigEntity(Types type)
        {
            Type = type;
        }

        public abstract string AsText();

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
    }

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
            var sections = getSectionsFromKVPs();
            insertEmptyRootSections();
            restoreTree();

            var configEntities = getEntities();
            updateFile();

            ////////////////////////////////////////

            List<Section> getSectionsFromKVPs()
            {
                var ss = new List<Section>();
                foreach (var section in KVPs.Select(kvp => kvp.Section).Distinct())
                {
                    ss.Add(section);
                }
                return ss;
            }

            void insertEmptyRootSections()
            {
                bool valid = true;
                do
                {
                    valid = true;
                    Section newEntity = null;
                    foreach (var section in sections)
                    {
                        if (!section.HasParentDirect(sections) && !section.IsRoot)
                        {
                            newEntity = section.GetParent();
                            valid = false;
                            break;
                        }
                    }
                    if (!valid)
                    {
                        sections.Add(newEntity);
                    }
                }
                while (!valid);
            }

            void restoreTree()
            {
                sections.Sort();
                for (int i = 0; i < sections.Count; i++)
                {
                    var section = sections[i];
                    var children = section.FindDirectChildren(sections.Skip(i)).ToArray();
                    foreach (var index in children.GetIndexes().Reverse())
                    {
                        sections.RemoveAt(index + i);
                    }
                    sections.InsertRange(i + 1, children.GetValues());
                }
            }

            List<ConfigEntity> getEntities()
            {
                var entities = new List<ConfigEntity>(sections.Count + KVPs.Count);
                foreach (var section in sections)
                {
                    entities.Add(section);
                    foreach (var kvp in KVPs)
                    {
                        if (kvp.Section.Equals(section))
                        {
                            entities.Add(kvp);
                        }
                    }
                }
                entities.Remove(Section.RootSection);

                return entities;
            }

            void updateFile()
            {
                _file.SetLength(0);
                var writer = new StreamWriter(_file, _encoding);
                foreach (var entity in configEntities)
                {
                    writer.WriteLine(entity.AsText());
                }
                writer.Flush();
                _file.Flush();
            }
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
