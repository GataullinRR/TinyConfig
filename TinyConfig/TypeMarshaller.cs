using System;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig
{
    public abstract class TypeMarshaller
    {
        public TypeMarshaller() { }

        public abstract Type ValueType { get; }
        public abstract bool IsAlwaysMultiline { get; }

        public abstract bool TryPack(object value, out ConfigValue result);
        public abstract bool TryUnpack(ConfigValue packed, out object result);
    }

    public abstract class OneLineTypeMarshaller<T> : TypeMarshaller
    {
        public override sealed Type ValueType { get; } = typeof(T);
        public override bool IsAlwaysMultiline { get; }

        public OneLineTypeMarshaller()
        {
            IsAlwaysMultiline = false;
        }
        public OneLineTypeMarshaller(bool isAlwaysMultiline)
        {
            IsAlwaysMultiline = isAlwaysMultiline;
        }

        public sealed override bool TryPack(object value, out ConfigValue result)
        {
            result = new ConfigValue(null, false);
            var isPacked = TryPack((T)value, out string valueText);
            if (isPacked)
            {
                result = new ConfigValue(valueText, IsAlwaysMultiline || valueText.Contains(Global.NL));
            }

            return isPacked;
        }
        public sealed override bool TryUnpack(ConfigValue packed, out object result)
        {
            var isOk = TryUnpack(packed.Value, out T specificResult);
            result = specificResult;

            return isOk;
        }

        public abstract bool TryPack(T value, out string result);
        public abstract bool TryUnpack(string packed, out T result);
    }
}
