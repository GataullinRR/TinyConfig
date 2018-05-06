using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig.Marshallers
{
    class ByteMarshaller : OneLineTypeMarshaller<byte>
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

    class SByteMarshaller : OneLineTypeMarshaller<sbyte>
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

    class Int16Marshaller : OneLineTypeMarshaller<short>
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

    class UInt16Marshaller : OneLineTypeMarshaller<ushort>
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

    class UInt32Marshaller : OneLineTypeMarshaller<uint>
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

    class Int64Marshaller : OneLineTypeMarshaller<long>
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

    class UInt64Marshaller : OneLineTypeMarshaller<ulong>
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

    class DoubleMarshaller : OneLineTypeMarshaller<double>
    {
        public override bool TryPack(double value, out string result)
        {
            result = value.ToStringInvariant();
            return true;
        }

        public override bool TryUnpack(string packed, out double result)
        {
            var parsed = packed.TryParseToDoubleInvariant();
            result = parsed.HasValue ? parsed.Value : default(double);

            return parsed.HasValue;
        }
    }
}
