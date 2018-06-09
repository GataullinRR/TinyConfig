using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig.Marshallers
{
    class Int32Marshaller : SpaceArraySeparatorTypeMarshaller<int>
    {
        public override bool TryPack(int value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out int result)
        {
            var parsed = packed.TryParseToInt32Invariant();
            result = parsed.HasValue ? parsed.Value : default(int);

            return parsed.HasValue;
        }
    }
}
