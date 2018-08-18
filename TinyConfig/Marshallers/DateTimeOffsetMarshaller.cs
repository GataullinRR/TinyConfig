using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig.Marshallers
{
    public class DateTimeOffsetMarshaller : SpaceArraySeparatorValueMarshaller<DateTimeOffset>
    {
        const string FORMAT = "o";

        public override bool TryPack(DateTimeOffset value, out string result)
        {
            result = value.ToString(FORMAT, CultureInfo.InvariantCulture);

            return true;
        }

        public override bool TryUnpack(string packed, out DateTimeOffset result)
        {
            return DateTimeOffset.TryParseExact(packed, FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result);
        }
    }
}
