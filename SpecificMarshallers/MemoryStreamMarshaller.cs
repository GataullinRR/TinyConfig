using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TinyConfig;
using Utilities.Extensions;

namespace SpecificMarshallers
{
    public class MemoryStreamMarshaller : ExactTypeMarshaller<MemoryStream>
    {
        public MemoryStreamMarshaller()
            : base(false, " ") { }

        public override bool TryPack(MemoryStream value, out string result)
        {
            var isFixedSize = value.IsFixedSize();
            result = $"{isFixedSize} {value.ToArray().ToBase64()}";

            return true;
        }

        public override bool TryUnpack(string packed, out MemoryStream result)
        {
            result = null;
            var isFixedSizeString = packed.TakeWhile(char.IsLetter).Aggregate();
            bool ok = bool.TryParse(isFixedSizeString, out bool isFixedSize);
            if (!ok)
            {
                return false;
            }

            var dataString = packed
                .SkipWhile(char.IsLetter)
                .SkipWhile(c => !char.IsLetterOrDigit(c))
                .TakeWhile(char.IsLetterOrDigit)
                .Aggregate();
            byte[] data = null;
            try
            {
                data = Convert.FromBase64String(dataString);
            }
            catch
            {
                return false;
            }

            if (isFixedSize)
            {
                result = new MemoryStream(data);
            }
            else
            {
                result = new MemoryStream();
                result.Write(data);
            }

            return true;
        }
    }
}
