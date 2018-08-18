using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig.Marshallers
{
    public class BooleanMarshaller : SpaceArraySeparatorValueMarshaller<bool>
    {
        public override bool TryPack(bool value, out string result)
        {
            result = value.ToString();
            return true;
        }

        public override bool TryUnpack(string packed, out bool result)
        {
            return bool.TryParse(packed, out result);
        }
    }
}
