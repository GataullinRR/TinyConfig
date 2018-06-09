using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig.Marshallers
{
    class StringMarshaller : TypeMarshaller<string>
    {
        public StringMarshaller() : base(true, null)
        {

        }

        public override bool TryPack(string value, out string result)
        {
            result = value;
            return true;
        }

        public override bool TryUnpack(string packed, out string result)
        {
            result = packed;
            return true;
        }
    }
}
