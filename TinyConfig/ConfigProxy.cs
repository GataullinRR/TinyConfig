using System;

namespace TinyConfig
{
    public class ConfigProxy<T>
    {
        public static implicit operator T(ConfigProxy<T> d)
        {
            return d.Value;
        }

        Action<T> _updater;
        T _Value;

        internal ConfigProxy(T value, Action<T> updater)
        {
            _updater = updater;
            Value = value;
        }

        public T Value
        {
            get => _Value;
            set => _updater(_Value = value);
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
