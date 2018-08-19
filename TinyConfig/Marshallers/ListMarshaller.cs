using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig.Marshallers
{
    public class ListMarshaller : ValueMarshaller
    {
        static bool isGenericList(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }
        static ValueMarshaller getMarshaller(Type type)
        {
            var m = ConfigAccessor.GetStandartValueMarshaller(getElementType(type));
            return m.GetType() == typeof(ListMarshaller) ? null : m;
        }
        static Type getElementType(Type type)
        {
            return type.GetGenericArguments()[0];
        }

        public ListMarshaller() 
            : base(t => isGenericList(t) && ConfigAccessor.IsValueSupported(getElementType(t)))
        {
        }

        public override bool TryPack(object value, out string result)
        {
            var ok = getMarshaller(value.GetType())
                .TryPack(((dynamic)value).ToArray(), out ConfigValue configValue);
            result = ok ? configValue.Value : null;

            return ok;
        }

        public override bool TryUnpack(string packed, Type supposedType, out object result)
        {
            var elementType = getElementType(supposedType);
            var ok = getMarshaller(supposedType)
                .TryUnpack(new ConfigValue(packed, false), supposedType, out result);
            result = ok
                ? constructList(result)
                : result;

            return ok;

            object constructList(object res)
            {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                if (res.GetType().IsArray)
                {
                    foreach (var item in (Array)res)
                    {
                        list.Add(Convert.ChangeType(item, elementType));
                    }
                }
                else
                {
                    list.Add(res);
                }
                return list;
            }
        }
    }
}
