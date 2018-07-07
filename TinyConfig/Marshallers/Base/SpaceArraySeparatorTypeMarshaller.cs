using System;

namespace TinyConfig
{
    abstract class SpaceArraySeparatorTypeMarshaller : TypeMarshaller
    {
        public SpaceArraySeparatorTypeMarshaller(Func<Type, bool> isTypeSupportedTester)
            : base(isTypeSupportedTester, false, " ")
        {

        }

        public SpaceArraySeparatorTypeMarshaller(
            Func<Type, bool> isTypeSupportedTester, bool isAlwaysMultiline, string arraySeparator)
            : base(isTypeSupportedTester, isAlwaysMultiline, arraySeparator)
        {

        }
    }

    abstract class SpaceArraySeparatorTypeMarshaller<T> : ExactTypeMarshaller<T>
    {
        public SpaceArraySeparatorTypeMarshaller()
            : base(false, " ")
        {

        }

        public SpaceArraySeparatorTypeMarshaller(bool isAlwaysMultiline, string arraySeparator)
            : base(isAlwaysMultiline, arraySeparator)
        {

        }
    }
}
