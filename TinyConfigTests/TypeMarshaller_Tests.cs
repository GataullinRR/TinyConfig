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
        public void TryPack_ArrayTest()
        {
            var m = new DoubleMarshaller();
            m.TryPack(new double[] { -1, 2, 3.4, 4.59, 0 },  out ConfigValue result);

            var actual = result.Value;
            var expected = "-1 2 3.4 4.59 0";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void TryPackString_ArrayTest()
        {
            var m = new StringMarshaller();
            Assert.Throws<Exception>(() => m.TryPack(new string[] { "one", "two!" }, out ConfigValue result));
        }
    }
}