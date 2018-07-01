using System;

namespace TinyConfig
{
    internal class ConfigKVP : IEquatable<ConfigKVP>
    {
        public string Key { get; }
        public ConfigValue Value { get; }
        public string Commentary { get; }

        public ConfigKVP(string key, ConfigValue value, string commentary)
        {
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
            return Key == other.Key && 
                   Value.Equals(other.Value);
        }
    }
}
