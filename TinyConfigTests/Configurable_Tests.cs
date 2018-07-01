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
        public void CreateConfig_WithCommentaryTest()
        {
            var config = Configurable.CreateConfig("CreateConfig_WithCommentaryTest", "SomeDir");
            var accessor = config.ReadValue(10, "SomeInt32").SetComment("Hello!");
            config.ReadValue(1.3, "SomeDouble");

            var actual = config.ToString();
            var expected = @"SomeInt32 =10\\Hello!
SomeDouble =1.3" + Global.NL;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual("Hello!", accessor.Commentary);
        }

        [Test()]
        public void CreateConfig_WithMultilineCommentaryTest()
        {
            var config = Configurable.CreateConfig("CreateConfig_WithMultilineCommentaryTest", "SomeDir");
            var accessor = config.ReadValue(-1, "SomeInt32").SetComment($"First line - {Global.NL}second");
            config.ReadValue(1.3e2, "SomeDouble");

            var actual = config.ToString();
            var expected = @"SomeInt32 =-1\\First line - second
SomeDouble =130" + Global.NL;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual("First line - second", accessor.Commentary);
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
        public void CreateAndModify_WithCommentaryTest()
        {
            var config = Configurable.CreateConfig("CreateAndModify_WithCommentaryTest", "SomeDir");
            config.ReadValue((byte)3, "SomeUInt8");
            var accessor = config.ReadValue(new int[] { 1, 2, 3 }, "SomeInt32Arr").SetComment("Hello!");
            config.ReadValue("Hello", "SomeString");

            var actual1 = config.ToString();
            var expected1 = @"SomeUInt8 =3
SomeInt32Arr =1 2 3\\Hello!
SomeString =#'Hello'" + Global.NL;

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual("Hello!", accessor.Commentary);

            accessor.SetComment("It is int[]");
            var actual2 = config.ToString();
            var expected2 = @"SomeUInt8 =3
SomeInt32Arr =1 2 3\\It is int[]
SomeString =#'Hello'" + Global.NL;

            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual("It is int[]", accessor.Commentary);
        }

        [Test()]
        public void CreateAndModify_RemoveCommentaryTest()
        {
            var config = Configurable.CreateConfig("CreateAndModify_RemoveCommentaryTest", "SomeDir");
            config.ReadValue((byte)3, "SomeUInt8").SetComment("Hi!");
            config.ReadValue(new int[] { 1, 2, 3 }, "SomeInt32Arr");
            var accessor = config.ReadValue("Hello", "SomeString").SetComment("Hello!");

            var actual1 = config.ToString();
            var expected1 = @"SomeUInt8 =3\\Hi!
SomeInt32Arr =1 2 3
SomeString =#'Hello'\\Hello!" + Global.NL;

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual("Hello!", accessor.Commentary);

            accessor.SetComment(null);
            var actual2 = config.ToString();
            var expected2 = @"SomeUInt8 =3\\Hi!
SomeInt32Arr =1 2 3
SomeString =#'Hello'" + Global.NL;

            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual(null, accessor.Commentary);
        }

        [Test()]
        public void CreateAndModifyArray_Test()
        {
            var config = Configurable.CreateConfig("CreateAndModifyArray");
            var accessor = config.ReadValue(new[] { 10, 20, 55 }, "SomeInt32Array");

            var expected = Global.Random.NextUnique(0, 1000, 10).ToArray();
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