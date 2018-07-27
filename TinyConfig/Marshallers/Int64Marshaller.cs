using Utilities.Extensions;

namespace TinyConfig.Marshallers
{
    public class Int64Marshaller : SpaceArraySeparatorTypeMarshaller<long>
    {
        public override bool TryPack(long value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out long result)
        {
            var parsed = packed.TryParseToInt64Invariant();
            result = parsed.HasValue ? parsed.Value : default(long);

            return parsed.HasValue;
        }
    }
}
