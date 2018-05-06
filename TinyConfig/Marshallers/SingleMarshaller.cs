using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig.Marshallers
{
    class SingleMarshaller : OneLineTypeMarshaller<float>
    {
        public override bool TryPack(float value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out float result)
        {
            var parsed = packed.TryParseToSingleInvariant();
            result = parsed.HasValue ? parsed.Value : default(float);

            return parsed.HasValue;
        }
    }
}
