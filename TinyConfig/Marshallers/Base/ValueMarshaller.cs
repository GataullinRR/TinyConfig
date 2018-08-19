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
            if (value == null)
            {
                result = new ConfigValue(Constants.NULL_VALUE, false);
                return true;
            }
            else
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
            }

            ///////////////////////////////////////

            bool tryPackAllElements(ref ConfigValue packed)
            {
                var arr = (Array)value;
                var packedValues = arr.ToEnumerable().Select(v =>
                {
#warning
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
                    valueText = escapeNull();
                    var isMultiline = IsAlwaysMultiline || valueText.Contains(Global.NL);
                    packed = new ConfigValue(valueText, isMultiline);
                }

                return isPacked;

                string escapeNull()
                {
                    var escaped = valueText;
                    var nullEntity = valueText.Find(Constants.NULL_VALUE);
                    if (nullEntity.Found)
                    {
                        var hasToBeEscaped = valueText
                            .Take(nullEntity.Index)
                            .SkipWhile(char.IsWhiteSpace)
                            .All(c => c == Constants.NULL_VALUE_ESCAPE_PERFIX);
                        if (hasToBeEscaped)
                        {
                            escaped = new Enumerable<char>()
                                {
                                    valueText.Take(nullEntity.Index),
                                    Constants.NULL_VALUE_ESCAPE_PERFIX,
                                    valueText.Skip(nullEntity.Index)
                                }.Aggregate();
                        }
                    }

                    return escaped;
                }
            }
        }
        internal bool TryUnpack(ConfigValue packed, Type supposedType, out object result)
        {
            if (ArraySeparator != null)
            {
                var dd = packed.Value.Split(ArraySeparator).Select(val =>
                {
                    var unpacked = tryUnpackWithNullEscaping(val, supposedType, out object specificResult);
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
                var unpacked = tryUnpackWithNullEscaping(packed.Value, supposedType, out object specificResult);
                result = specificResult;
                return unpacked;
            }

            ///////////////////////////////////

            bool tryUnpackWithNullEscaping(string pckd, Type type, out object rslt)
            {
                rslt = null;
                var nullEntity = pckd.Find(Constants.NULL_VALUE);
                var isNull = false;
                if (nullEntity.Found)
                {
                    var gap = pckd.Take(nullEntity.Index).SkipWhile(char.IsWhiteSpace);
                    isNull = gap.IsEmpty();

                    if (!isNull && gap.All(c => c == Constants.NULL_VALUE_ESCAPE_PERFIX))
                    {
                        pckd = new Enumerable<char>()
                        {
                            pckd.Take(nullEntity.Index - 1),
                            pckd.Skip(nullEntity.Index),
                        }.Aggregate();
                    }
                }
                return isNull
                    ? true
                    : TryUnpack(pckd, type, out rslt);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Always not null.</param>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool TryPack(object value, out string result);
        public abstract bool TryUnpack(string packed, Type supposedType, out object result);
    }
}
