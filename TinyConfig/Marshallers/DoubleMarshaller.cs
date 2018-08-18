using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig.Marshallers
{
    public class DoubleMarshaller : SpaceArraySeparatorValueMarshaller<double>
    {
        public override bool TryPack(double value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out double result)
        {
            var parsed = packed.TryParseToDoubleInvariant();
            result = parsed.HasValue ? parsed.Value : default(double);

            return parsed.HasValue;
        }
    }
}
