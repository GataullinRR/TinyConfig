using Utilities.Extensions;

namespace TinyConfig.Marshallers
{
    public class Int16Marshaller : SpaceArraySeparatorValueMarshaller<short>
    {
        public override bool TryPack(short value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out short result)
        {
            var parsed = packed.TryParseToInt16Invariant();
            result = parsed.HasValue ? parsed.Value : default(short);

            return parsed.HasValue;
        }
    }
}
