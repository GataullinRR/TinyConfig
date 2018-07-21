using System;
using Utilities.Extensions;

namespace TinyConfig
{
    internal class Section : IEquatable<Section>
    {
        public string FullName { get; }
        public int Order { get; }
        public bool IsRoot { get; }

        public Section(string fullName)
        {
            FullName = fullName;
            Order = FullName?.FindAll(Constants.SUBSECTION_SEPARATOR)?.Count ?? 0;
            IsRoot = FullName == null;
        }

        public bool IsInsideSection(string sectionFullName)
        {
            var isArgumentValid = sectionFullName != null
                && sectionFullName.Length > 0
                && !sectionFullName.StartsWith(Constants.SUBSECTION_SEPARATOR)
                && !sectionFullName.EndsWith(Constants.SUBSECTION_SEPARATOR);
            if (!isArgumentValid)
            {
                throw new ArgumentException();
            }

            if (FullName == null)
            {
                return false;
            }
            else
            {
                return FullName.StartsWith(sectionFullName)
                    && FullName.Remove(0, sectionFullName.Length).StartsWith(Constants.SUBSECTION_SEPARATOR);
            }
        }

        public bool Equals(Section other)
        {
            return FullName == other.FullName
                && Order == other.Order;
        }

        public override string ToString()
        {
            return Constants.SECTION_HEADER_OPEN_MARK + FullName + Constants.SECTION_HEADER_CLOSE_MARK;
        }
    }

    internal class ConfigKVP : IEquatable<ConfigKVP>
    {
        public Section Section { get; }
        public string Key { get; }
        public ConfigValue Value { get; }
        public string Commentary { get; }

        public ConfigKVP(string key, ConfigValue value, string commentary)
            :this(new Section(null), key, value, commentary)
        {

        }
        public ConfigKVP(Section section, string key, ConfigValue value, string commentary)
        {
            Section = section;
            Key = key;
            Value = value;
            Commentary = commentary;
        }



        public override string ToString()
        {
            var shieldedValue = Value.Value
                .Replace(Constants.BLOCK_MARK.ToString(), string.Concat(Constants.BLOCK_MARK, Constants.BLOCK_MARK));
            var withoutComment = Value.IsMultiline
                ? $"{Key} {Constants.KVP_SEPERATOR}{Constants.MULTILUNE_VALUE_MARK}" +
                    $"{Constants.BLOCK_MARK}{shieldedValue}{Constants.BLOCK_MARK}"
                : $"{Key} {Constants.KVP_SEPERATOR}{shieldedValue}";
            return string.IsNullOrEmpty(Commentary) 
                ? withoutComment
                : $"{withoutComment}{Constants.COMMENT_SEPARATOR}{Commentary}";
        }

        public override int GetHashCode()
        {
            return new { Key, Value }.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ConfigKVP kvp)
            {
                return Equals(kvp);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ConfigKVP other)
        {
            return Section.Equals(other.Section)
                && Key == other.Key
                && Value.Equals(other.Value)
                && Commentary == other.Commentary;
        }
    }
}
