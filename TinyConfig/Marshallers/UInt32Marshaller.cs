using Utilities.Extensions;

namespace TinyConfig.Marshallers
{
    public class UInt32Marshaller : SpaceArraySeparatorTypeMarshaller<uint>
    {
        public override bool TryPack(uint value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out uint result)
        {
            var parsed = packed.TryParseToUInt32Invariant();
            result = parsed.HasValue ? parsed.Value : default(uint);

            return parsed.HasValue;
        }
    }
}
