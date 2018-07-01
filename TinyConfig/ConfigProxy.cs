using System;

namespace TinyConfig
{
    public class ConfigProxy<T>
    {
        public static implicit operator T(ConfigProxy<T> d)
        {
            return d.Value;
        }

        readonly Action<T> _valueUpdater;
        readonly Func<string, string> _commentaryUpdater;
        string _Commentary;
        T _Value;

        public T Value
        {
            get => _Value;
            set => _valueUpdater(_Value = value);
        }

        public string Commentary
        {
            get => _Commentary;
            set => _Commentary = _commentaryUpdater(value);
        }

        internal ConfigProxy(T value, string commentary, Action<T> valueUpdater, Func<string, string> commentaryUpdater)
        {
            _Value = value;
            _Commentary = commentary;
            _valueUpdater = valueUpdater;
            _commentaryUpdater = commentaryUpdater;
        }

        public ConfigProxy<T> SetComment(string commentary)
        {
            Commentary = commentary;
            return this;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }
    }
}
