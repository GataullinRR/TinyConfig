using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using TinyConfig;
using Utilities;
using Utilities.Extensions;

namespace SpecificMarshallers
{
    public class TryParseMarshaller<T> : ExactTypeMarshaller<T>
    {
        delegate bool TryParseDelegate(string packed, out T result);

        static readonly TryParseDelegate TRY_PARCE;

        static TryParseMarshaller()
        {
            var type = typeof(T);
            TRY_PARCE = (TryParseDelegate)type
                .GetMethod("TryParse")
                .CreateDelegate(typeof(TryParseDelegate));
        }

        public override bool TryPack(T value, out string result)
        {
            result = value.ToString();

            return true;
        }

        public override bool TryUnpack(string packed, out T result) => TRY_PARCE(packed, out result);
    }
}
