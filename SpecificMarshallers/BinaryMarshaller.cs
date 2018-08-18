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
    public class BinaryMarshaller<T> : ExactValueMarshaller<T>
    {
        readonly BinaryFormatter _serializer = new BinaryFormatter();

        public override bool TryPack(T value, out string result)
        {
            try
            {
                result = _serializer.Serialize(value).ToBase64();
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public override bool TryUnpack(string packed, out T result)
        {
            try
            {
                result = (T)_serializer.Deserialize(Convert.FromBase64String(packed));
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
