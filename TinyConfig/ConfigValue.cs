using System;

namespace TinyConfig
{
    class ConfigValue : IEquatable<ConfigValue>
    {
        public string Value { get; }
        public bool IsMultiline { get; }

        public ConfigValue(string value, bool isMultiline)
        {
            Value = value;
            IsMultiline = isMultiline;
        }

        public override int GetHashCode()
        {
            return new { Value, IsMultiline }.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ConfigValue value)
            {
                return Equals(value);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ConfigValue other)
        {
            return Value == other.Value &&
                   IsMultiline == other.IsMultiline;
        }

        public override string ToString()
        {
            return $"{nameof(IsMultiline)}:{IsMultiline} {nameof(Value)}:{Value}";
        }
    }
}
