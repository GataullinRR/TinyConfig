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
    public class TimeSpanMarshaller : SpaceArraySeparatorValueMarshaller<TimeSpan>
    {
        const string FORMAT = "c";

        public override bool TryPack(TimeSpan value, out string result)
        {
            result = value.ToString(FORMAT, CultureInfo.InvariantCulture);

            return true;
        }

        public override bool TryUnpack(string packed, out TimeSpan result)
        {
            return TimeSpan.TryParseExact(packed, FORMAT, CultureInfo.InvariantCulture, TimeSpanStyles.None, out result);
        }
    }
}
