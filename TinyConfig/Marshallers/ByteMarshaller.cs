using Utilities.Extensions;

namespace TinyConfig.Marshallers
{
    public class ByteMarshaller : SpaceArraySeparatorValueMarshaller<byte>
    {
        public override bool TryPack(byte value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out byte result)
        {
            var parsed = packed.TryParseToByteInvariant();
            result = parsed.HasValue ? parsed.Value : default(byte);

            return parsed.HasValue;
        }
    }
}
