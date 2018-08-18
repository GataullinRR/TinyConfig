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
    public class DateTimeMarshaller : SpaceArraySeparatorValueMarshaller<DateTime>
    {
        const string FORMAT = "o";

        public override bool TryPack(DateTime value, out string result)
        {
            result = value.ToString(FORMAT, CultureInfo.InvariantCulture);

            return true;
        }

        public override bool TryUnpack(string packed, out DateTime result)
        {
            return DateTime.TryParseExact(packed, FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result);
        }
    }
}
