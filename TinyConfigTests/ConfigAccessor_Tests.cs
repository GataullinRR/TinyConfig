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
        class VectorMarshaller : ExactTypeMarshaller<V2>
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
                        .Skip(1)
                        .TakeWhile(c => !char.IsWhiteSpace(c))
                        .Aggregate()
                        .ParseToDoubleInvariant();
                }
            }
        }

        class MyIntMarshaller : ExactTypeMarshaller<int>
        {
            public override bool TryPack(int value, out string result)
            {
                result = "DEC: " + value.ToString();

                return true;
            }

            public override bool TryUnpack(string packed, out int result)
            {
                result = packed.Skip(5).Aggregate().ParseToInt32Invariant();

                return true;
            }
        }

        [Test()]
        public void AddMarshaller_Test()
        {
            var config = Configurable.CreateConfig("AddMarshaller").Clear()
                .AddMarshaller<VectorMarshaller>();
            config.ReadValue(10, "SomeInt32");
            config.ReadValue(new V2(-9, 9), "SomeV2");

            var actual = config.ToString();
            var expected = @"SomeInt32 =10
SomeV2 =X:-9 Y:9
";

            Assert.AreEqual(expected, actual);
        }
        
        [Test()]
        public void OverrideMarshaller_Test()
        {
            var config = Configurable.CreateConfig("OverrideMarshaller_Test").Clear()
                .AddMarshaller<MyIntMarshaller>();
            config.ReadValue(10, "SomeInt32");

            var actual = config.ToString();
            var expected = @"SomeInt32 =DEC: 10
";

            Assert.AreEqual(expected, actual);
        }
    }
}