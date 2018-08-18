using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig.Marshallers
{
    public class EnumMarshaller : SpaceArraySeparatorValueMarshaller
    {
        public EnumMarshaller() 
            : base(t => t.IsEnum)
        {

        }

        public override bool TryPack(object value, out string result)
        {
            result = value.ToString();

            return true;
        }
        public override bool TryUnpack(string packed, Type supposedType, out object result)
        {
            return CommonUtils.Try(() => Enum.Parse(supposedType, packed), out result);
        }
    }
}
