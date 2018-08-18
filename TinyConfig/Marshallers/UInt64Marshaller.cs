using Utilities.Extensions;

namespace TinyConfig.Marshallers
{
    public class UInt64Marshaller : SpaceArraySeparatorValueMarshaller<ulong>
    {
        public override bool TryPack(ulong value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out ulong result)
        {
            var parsed = packed.TryParseToUInt64Invariant();
            result = parsed.HasValue ? parsed.Value : default(ulong);

            return parsed.HasValue;
        }
    }
}
