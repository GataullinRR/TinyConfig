using Utilities.Extensions;

namespace TinyConfig.Marshallers
{
    public class SByteMarshaller : SpaceArraySeparatorTypeMarshaller<sbyte>
    {
        public override bool TryPack(sbyte value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out sbyte result)
        {
            var parsed = packed.TryParseToSByteInvariant();
            result = parsed.HasValue ? parsed.Value : default(sbyte);

            return parsed.HasValue;
        }
    }
}
