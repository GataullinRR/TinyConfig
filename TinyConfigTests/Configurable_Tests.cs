using NUnit.Framework;
using TinyConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;
using System.IO;

namespace TinyConfig.Tests
{
    [TestFixture()]
    public class Configurable_Tests
    {
        [Test()]
        public void CreateConfig_Test()
        {
            var config = Configurable.CreateConfig("Test", "SomeDir");
            config.ReadValue(10, "SomeInt32");
            config.ReadValue(1.3, "SomeDouble");
        }

        [Test()]
        public void CreateConfig_ArrayValueTest()
        {
            var config = Configurable.CreateConfig("CreateConfig_ArrayValueTest");
            config.ReadValue(10, "SomeInt32");
            var doubleAccessor = config.ReadValue(new double[] { -1, 2, 2.345 }, "SomeDoubleArr");

            var actual = config.ToString();
            var expected = @"SomeInt32 =10
SomeDoubleArr =-1 2 2.345" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void CreateAndModify_Test()
        {
            var config = Configurable.CreateConfig("CreateAndModify", "");
            config.ReadValue(10, "SomeInt32");
            var accessor = config.ReadValue("Hello ", "SomeString");
            config.ReadValue(3, "SomeByte");

            accessor.Value = $"Hello {Global.Random.NextENWord()}";
        }

        [Test()]
        public void CreateAndModifyArray_Test()
        {
            var config = Configurable.CreateConfig("CreateAndModifyArray");
            var accessor = config.ReadValue(new[] { 10, 20, 55 }, "SomeInt32Array");

            var expected = Global.Random.GenerateUnique(0, 1000, 10).ToArray();
            accessor.Value = expected;
            var actual = accessor.Value;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void CreateTwoFieldsWithSameName_Test()
        {
            var config = Configurable.CreateConfig("CreateTwoFieldsWithSameName", "");
            config.ReadValue("ABC", "SomeValue");
            config.ReadValue(123, "SomeValue");

            var actual = config.ToString();
            var expected = "SomeValue =#'ABC'" + Global.NL;

            Assert.AreEqual(expected, actual);
        }
    }
}