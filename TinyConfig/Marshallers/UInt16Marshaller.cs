using Utilities.Extensions;

namespace TinyConfig.Marshallers
{
    public class UInt16Marshaller : SpaceArraySeparatorValueMarshaller<ushort>
    {
        public override bool TryPack(ushort value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out ushort result)
        {
            var parsed = packed.TryParseToUInt16Invariant();
            result = parsed.HasValue ? parsed.Value : default(ushort);

            return parsed.HasValue;
        }
    }
}
