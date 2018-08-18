using System;

namespace TinyConfig
{
    public abstract class ExactValueMarshaller : ValueMarshaller
    {
        public ExactValueMarshaller(Type type)
            :base(t => t == type)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }
        }

        public ExactValueMarshaller(Type type, bool isAlwaysMultiline, string arraySeparator)
            : base(t => t == type, isAlwaysMultiline, arraySeparator)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }
        }
    }

    public abstract class ExactValueMarshaller<T> : ExactValueMarshaller
    {
        public ExactValueMarshaller()
            : this(false, null)
        {

        }
        public ExactValueMarshaller(bool isAlwaysMultiline, string arraySeparator)
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
