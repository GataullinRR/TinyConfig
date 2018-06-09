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
        public abstract string ArraySeparator { get; }

        internal abstract bool TryPack(object value, out ConfigValue result);
        internal abstract bool TryUnpack(ConfigValue packed, out object result);
    }

    class ArrayCastHelper<T>
    {
        public static explicit operator T(ArrayCastHelper<T> dd)
        {
            return dd._value[0];
        }
        public static explicit operator T[] (ArrayCastHelper<T> dd)
        {
            return dd._value;
        }

        readonly T[] _value;

        public ArrayCastHelper(T[] value)
        {
            _value = value;
        }
    }

    public abstract class TypeMarshaller<T> : TypeMarshaller
    {
        public override sealed Type ValueType { get; } = typeof(T);
        public override bool IsAlwaysMultiline { get; }
        public override string ArraySeparator { get; } = null;

        public TypeMarshaller()
        {
            IsAlwaysMultiline = false;
        }
        public TypeMarshaller(bool isAlwaysMultiline, string arraySeparator)
        {
            IsAlwaysMultiline = isAlwaysMultiline;
            ArraySeparator = arraySeparator;
        }

        internal sealed override bool TryPack(object value, out ConfigValue result)
        {
            result = null;
            if (value.GetType().IsArray && ValueType != value.GetType())
            {
                if (ArraySeparator == null)
                {
                    throw new Exception();
                }

                var arr = (Array)value;
                var packedValues = arr.ToEnumerable().Select(v =>
                {
                    var isOk = TryPack(v, out ConfigValue configValue);
                    return isOk ? configValue : null;
                }).ToArray();
                if (packedValues.All(pv => pv != null))
                {
                    var val = packedValues
                        .Select(cv => cv.Value)
                        .Aggregate((acc, v) => acc + ArraySeparator + v);
                    var isMultiline = packedValues
                        .Any(cv => cv.IsMultiline);
                    result = new ConfigValue(val, isMultiline);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                var isPacked = TryPack((T)value, out string valueText);
                if (isPacked)
                {
                    var isMultiline = IsAlwaysMultiline || valueText.Contains(Global.NL);
                    result = new ConfigValue(valueText, isMultiline);
                }

                return isPacked;
            }
        }
        internal sealed override bool TryUnpack(ConfigValue packed, out object result)
        {
            if (ArraySeparator != null)
            {
                var dd = packed.Value.Split(ArraySeparator).Select(val =>
                {
                    var unpacked = TryUnpack(val, out T specificResult);
                    return new { unpacked, specificResult };
                });

                if (dd.All(v => v.unpacked))
                {
                    result = new ArrayCastHelper<T>(dd.Select(v => v.specificResult).ToArray());
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            else
            {
                var unpacked = TryUnpack(packed.Value, out T specificResult);
                result = specificResult;
                return unpacked;
            }
        }

        public abstract bool TryPack(T value, out string result);
        public abstract bool TryUnpack(string packed, out T result);
    }

    abstract class SpaceArraySeparatorTypeMarshaller<T> : TypeMarshaller<T>
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
