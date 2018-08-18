using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TinyConfig;
using Utilities;
using Utilities.Extensions;

namespace SpecificMarshallers
{
    public class IPEndPointMarshaller : ExactValueMarshaller<IPEndPoint>
    {
        public override bool TryPack(IPEndPoint value, out string result)
        {
            result = $"{value.Address}:{value.Port}";

            return true;
        }

        public override bool TryUnpack(string packed, out IPEndPoint result)
        {
            var ipAndPort = packed.Split(":");
            if (ipAndPort.Length < 2)
            {
                result = default;
                return false;
            }
            else
            {
                var ipOk = IPAddress.TryParse(ipAndPort[0], out IPAddress ip);
                var portOk = ushort.TryParse(ipAndPort[1], out ushort port);
                var ok = ipOk && portOk;
                result = ok
                    ? new IPEndPoint(ip, port)
                    : null;

                return ok;
            }
        }
    }
}
