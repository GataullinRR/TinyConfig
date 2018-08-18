using System;

namespace TinyConfig
{
    public abstract class ObjectMarshaller
    {
        readonly Func<Type, bool> _isTypeSupportedTester;

        public ObjectMarshaller(Func<Type, bool> isTypeSupportedTester)
        {
            _isTypeSupportedTester = isTypeSupportedTester;
        }

        internal protected abstract bool TryPack(IConfigAccessorProxy configAccessor, object value);
        internal protected abstract bool TryUnpack(IConfigAccessorProxy configAccessor, Type supposedType, out object result);

        internal bool IsTypeSupported(Type type)
        {
            return _isTypeSupportedTester(type);
        }
    }
}
