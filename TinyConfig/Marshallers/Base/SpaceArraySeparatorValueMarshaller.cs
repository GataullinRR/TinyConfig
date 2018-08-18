using System;

namespace TinyConfig
{
    public abstract class SpaceArraySeparatorValueMarshaller : ValueMarshaller
    {
        public SpaceArraySeparatorValueMarshaller(Func<Type, bool> isTypeSupportedTester)
            : base(isTypeSupportedTester, false, " ")
        {

        }

        public SpaceArraySeparatorValueMarshaller(
            Func<Type, bool> isTypeSupportedTester, bool isAlwaysMultiline, string arraySeparator)
            : base(isTypeSupportedTester, isAlwaysMultiline, arraySeparator)
        {

        }
    }

    public abstract class SpaceArraySeparatorValueMarshaller<T> : ExactValueMarshaller<T>
    {
        public SpaceArraySeparatorValueMarshaller()
            : base(false, " ")
        {

        }

        public SpaceArraySeparatorValueMarshaller(bool isAlwaysMultiline, string arraySeparator)
            : base(isAlwaysMultiline, arraySeparator)
        {

        }
    }
}
