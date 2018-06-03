using NUnit.Framework;
using TinyConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;
using Vectors;

namespace TinyConfig.Tests
{
    [TestFixture()]
    public class ConfigAccessor_Tests
    {
        class VectorMarshaller : OneLineTypeMarshaller<V2>
        {
            public override bool TryPack(V2 value, out string result)
            {
                result = $"X:{value.X.ToStringInvariant()} Y:{value.Y.ToStringInvariant()}";

                return true;
            }

            public override bool TryUnpack(string packed, out V2 result)
            {
                var xy = packed.Split(" ");
                var x = extractValue(xy[0]);
                var y = extractValue(xy[1]);
                result = new V2(x, y);

                return true;

                double extractValue(string str)
                {
                    return str
                        .SkipWhile(c => c != ':')
                        .TakeWhile(c => !char.IsWhiteSpace(c))
                        .Aggregate()
                        .ParseToDoubleInvariant();
                }
            }
        }


        [Test()]
        public void AddMarshaller_Test()
        {
            var config = Configurable.CreateConfig("AddMarshaller").AddMarshaller<VectorMarshaller>();
            config.ReadValue(10, "SomeInt32");
            config.ReadValue(new V2(-9, 9), "SomeV2");
        }
    }
}