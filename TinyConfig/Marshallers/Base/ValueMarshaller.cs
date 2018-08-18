using System;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig
{
    public abstract class ValueMarshaller
    {
        readonly Func<Type, bool> _isTypeSupportedTester;

        protected bool IsAlwaysMultiline { get; }
        protected string ArraySeparator { get; }

        public ValueMarshaller(Func<Type, bool> isTypeSupportedTester)
            : this(isTypeSupportedTester, false, null)
        {

        }
        public ValueMarshaller(Func<Type, bool> isTypeSupportedTester, bool isAlwaysMultiline, string arraySeparator)
        {
            _isTypeSupportedTester = isTypeSupportedTester ?? throw new ArgumentNullException();
            IsAlwaysMultiline = isAlwaysMultiline;
            ArraySeparator = arraySeparator;
        }

        public bool IsTypeSupported(Type type)
        {
            return _isTypeSupportedTester(type);
        }

        internal bool TryPack(object value, out ConfigValue result)
        {
            result = null;
            var valueType = value.GetType();
            if (valueType.IsArray && !IsTypeSupported(valueType))
            {
                if (ArraySeparator == null)
                {
                    throw new ArrayPackingNotSupportedException();
                }

                return tryPackAllElements(ref result);
            }
            else if (IsTypeSupported(valueType))
            {
                return tryPack(ref result);
            }
            else
            {
                return false;
            }

            ///////////////////////////////////////

            bool tryPackAllElements(ref ConfigValue packed)
            {
                var arr = (Array)value;
                var packedValues = arr.ToEnumerable().Select(v =>
                {
                    var isOk = TryPack(v, out ConfigValue configValue);
                    return isOk ? configValue : null;
                }).ToArray();
                if (packedValues.IsNotEmpty() && packedValues.All(pv => pv != null))
                {
                    var val = packedValues
                        .Select(cv => cv.Value)
                        .Aggregate((acc, v) => acc + ArraySeparator + v);
                    var isMultiline = packedValues
                        .Any(cv => cv.IsMultiline);
                    packed = new ConfigValue(val, isMultiline);

                    return true;
                }
                else if (packedValues.IsEmpty())
                {
                    packed = new ConfigValue("", false);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            bool tryPack(ref ConfigValue packed)
            {
                var isPacked = TryPack(value, out string valueText);
                if (isPacked)
                {
                    var isMultiline = IsAlwaysMultiline || valueText.Contains(Global.NL);
                    packed = new ConfigValue(valueText, isMultiline);
                }

                return isPacked;
            }
        }
        internal bool TryUnpack(ConfigValue packed, Type supposedType, out object result)
        {
            if (ArraySeparator != null)
            {
                var dd = packed.Value.Split(ArraySeparator).Select(val =>
                {
                    var unpacked = TryUnpack(val, supposedType, out object specificResult) && 
                                   supposedType.IsAssignableFrom(specificResult?.GetType());
                    return new { unpacked, specificResult };
                });

                if (dd.All(v => v.unpacked))
                {
                    result = dd.Select(v => v.specificResult).ToArray();
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
                var unpacked = TryUnpack(packed.Value, supposedType, out object specificResult);
                result = specificResult;
                return unpacked;
            }
        }

        public abstract bool TryPack(object value, out string result);
        public abstract bool TryUnpack(string packed, Type supposedType, out object result);
    }
}
