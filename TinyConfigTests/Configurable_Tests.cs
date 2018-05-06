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
        public void CreateAndModify_Test()
        {
            var config = Configurable.CreateConfig("CreateAndModify", "");
            config.ReadValue(10, "SomeInt32");
            var accessor = config.ReadValue("Hello ", "SomeString");
            config.ReadValue(3, "SomeByte");

            accessor.Value = $"Hello {Global.Random.NextENWord()}";
        }

        [Test()]
        public void CreateTwoFieldsWithSameName_Test()
        {
            var config = Configurable.CreateConfig("CreateTwoFieldsWithSameName", "");
            config.ReadValue("ABC", "SomeValue");
            config.ReadValue(123, "SomeValue");

            var actual = config.ToString();
            var expected = "SomeValue = #'ABC'" + Global.NL;

            Assert.AreEqual(expected, actual);
        }
    }
}