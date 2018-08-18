using NUnit.Framework;
using TinyConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;
using TinyConfig.Marshallers;

namespace TinyConfig.Tests
{
    [TestFixture()]
    public class TypeMarshaller_Tests
    {
        [Test()]
        public void TryPack_DoubleArray()
        {
            var m = new DoubleMarshaller();
            m.TryPack(new double[] { -1, 2, 3.4, 4.59, 0 },  out ConfigValue result);

            var actual = result.Value;
            var expected = "-1 2 3.4 4.59 0";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void TryPack_DoubleNaN()
        {
            var m = new DoubleMarshaller();
            Assert.True(m.TryPack(double.NaN, out ConfigValue result));

            var actual = result.Value;
            var expected = "NaN";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void TryPack_DoubleInifinity()
        {
            var m = new DoubleMarshaller();
            Assert.True(m.TryPack(double.NegativeInfinity, out ConfigValue result));

            var actual = result.Value;
            var expected = "-Infinity";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void TryUnpack_DoubleInifinity()
        {
            var m = new DoubleMarshaller();
            Assert.True(m.TryUnpack("-Infinity", out double actual));

            Assert.AreEqual(double.NegativeInfinity, actual);
        }

        [Test()]
        public void TryPack_StringArray()
        {
            var m = new StringMarshaller();
            Assert.Throws<ArrayPackingNotSupportedException>(() => m.TryPack(new string[] { "one", "two!" }, out ConfigValue result));
        }
    }
}