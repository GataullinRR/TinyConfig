using System;
using System.Collections.Generic;
using System.Reflection;

namespace TinyConfig.Marshallers
{
    public class FlatStructObjectMarshaller : ObjectMarshaller
    {
        public FlatStructObjectMarshaller() 
            : base(t => (t.Attributes & TypeAttributes.Serializable) != 0 && t.IsValueType)
        {

        }

        internal protected override bool TryPack(IConfigAccessorProxy configAccessor, object value)
        {
            return pack();

            bool pack()
            {
                var t = value.GetType();
                foreach (var field in getFields(t))
                {
                    var v = field.GetValue(value);
                    var hasMarshaller = configAccessor.HasValueMarshaller(field.FieldType);
                    if (hasMarshaller)
                    {
                        configAccessor.WriteValue(field.FieldType, v, field.Name);
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal protected override bool TryUnpack(IConfigAccessorProxy configAccessor, Type supposedType, out object result)
        {
            result = Activator.CreateInstance(supposedType);

            foreach (var field in getFields(supposedType))
            {
                var hasMarshaller = configAccessor.HasValueMarshaller(field.FieldType);
                if (hasMarshaller)
                {
                    var value = configAccessor.ReadValue(field.FieldType, field.Name);
                    field.SetValue(result, value.Value);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        IEnumerable<FieldInfo> getFields(Type t)
        {
            var filter = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return t.GetFields(filter);
        }
    }
}
