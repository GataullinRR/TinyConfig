using System;

namespace TinyConfig
{
    class ConfigKVP : IEquatable<ConfigKVP>
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
