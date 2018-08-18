using System;

namespace TinyConfig
{
    public class ConfigProxy<T>
    {
        public static implicit operator T(ConfigProxy<T> d)
        {
            return d.Value;
        }

        public static bool operator ==(ConfigProxy<T> l, ConfigProxy<T> r)
        {
            return ReferenceEquals(l, r);
        }
        public static bool operator !=(ConfigProxy<T> l, ConfigProxy<T> r)
        {
            return !(l == r);
        }

        readonly Action _remove;
        readonly Action<T> _valueUpdater;
        readonly Action<string> _commentaryUpdater;
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
            set => _commentaryUpdater(_Commentary = value);
        }

        public bool IsRead { get; }

        internal ConfigProxy(T value, string commentary, bool isRead, 
            Action<T> valueUpdater, Action<string> commentaryUpdater, Action remove)
        {
            _Value = value;
            _Commentary = commentary;
            IsRead = isRead;
            _valueUpdater = valueUpdater;
            _commentaryUpdater = commentaryUpdater;
            _remove = remove;
        }

        public ConfigProxy<T> SetComment(string commentary)
        {
            Commentary = commentary;
            return this;
        }

        internal void Remove()
        {
            _remove();
        }

        //internal ConfigProxy<TTo> CastTo<TTo>()
        //    where TTo : T
        //{
        //    return new ConfigProxy<TTo>((TTo)Value, Commentary, IsRead, v => _valueUpdater(v), _commentaryUpdater);
        //}
        internal ConfigProxy<object> CastToRoot()
        {
            return new ConfigProxy<object>(Value, Commentary, IsRead, v => _valueUpdater((T)v), _commentaryUpdater, _remove);
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
