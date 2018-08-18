using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TinyConfig;

namespace SpecificMarshallers
{
    public class IPAddressMarshaller : ExactValueMarshaller<IPAddress>
    {
        public IPAddressMarshaller()
            : base(false, " ") { }

        public override bool TryPack(IPAddress value, out string result)
        {
            result = value?.ToString();
            return result != null;
        }

        public override bool TryUnpack(string packed, out IPAddress result)
        {
            if (packed.Count(c => c == '.') < 3)
            {
                result = null;
                return false;
            }

            return IPAddress.TryParse(packed, out result);
        }
    }
}
