using System;

namespace TinyConfig
{
    public abstract class ExactTypeMarshaller : ValueMarshaller
    {
        public ExactTypeMarshaller(Type type)
            :base(t => t == type)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }
        }

        public ExactTypeMarshaller(Type type, bool isAlwaysMultiline, string arraySeparator)
            : base(t => t == type, isAlwaysMultiline, arraySeparator)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }
        }
    }

    public abstract class ExactTypeMarshaller<T> : ExactTypeMarshaller
    {
        public ExactTypeMarshaller()
            : this(false, null)
        {

        }
        public ExactTypeMarshaller(bool isAlwaysMultiline, string arraySeparator)
            : base(typeof(T), isAlwaysMultiline, arraySeparator)
        {

        }

        public override sealed bool TryPack(object value, out string result)
        {
            return TryPack((T)value, out result);
        }
        public override sealed bool TryUnpack(string packed, Type supposedType, out object result)
        {
            var unpacked = TryUnpack(packed, out T specificResult);
            result = specificResult;

            return unpacked;
        }

        public abstract bool TryPack(T value, out string result);
        public abstract bool TryUnpack(string packed, out T result);
    }
}
