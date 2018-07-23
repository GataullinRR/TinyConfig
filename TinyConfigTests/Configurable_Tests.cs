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
        public void OpenSameConfigAsReadOnly_ConcurrentAccessTest()
        {
            var config1 = Configurable.CreateConfig("OpenSameConfigAsReadOnly_ConcurrentAccessTest", "Access", ConfigAccess.READ_ONLY);
            var config2 = Configurable.CreateConfig("OpenSameConfigAsReadOnly_ConcurrentAccessTest", "Access", ConfigAccess.READ_ONLY);
            var accessor1 = config1.ReadValue(5, "SomeInt32");
            var accessor2 = config1.ReadValue(15, "SomeInt32");

            var actual1 = config1.ToString();
            var expected1 = @"" + Global.NL;
            var actual2 = config2.ToString();
            var expected2 = @"" + Global.NL;

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual(5, accessor1.Value);
            Assert.AreEqual(15, accessor2.Value);
        }

        [Test()]
        public void OpenSameConfigAsReadWrite_ConcurrentAccessTest()
        {
            Configurable.CreateConfig("OpenSameConfigAsReadWrite_ConcurrentAccessTest", "Access", ConfigAccess.READ_WRITE);
            Assert.Throws<InvalidOperationException>(() =>
                Configurable.CreateConfig("OpenSameConfigAsReadWrite_ConcurrentAccessTest", "Access", ConfigAccess.READ_WRITE));
        }

        [Test()]
        public void OpenSameConfig_ConcurrentAccessTest()
        {
            Configurable.CreateConfig("OpenSameConfig_ConcurrentAccessTest1", "Access", ConfigAccess.READ_ONLY);
            Configurable.CreateConfig("OpenSameConfig_ConcurrentAccessTest1", "Access", ConfigAccess.READ_ONLY);
            Assert.Throws<InvalidOperationException>(() => 
                Configurable.CreateConfig("OpenSameConfig_ConcurrentAccessTest1", "Access", ConfigAccess.READ_WRITE));
        }

        [Test()]
        public void OpenConfigAsReadOnly_AccessTest()
        {
            var config = Configurable.CreateConfig("OpenConfigAsReadOnly_AccessTest", "Access", ConfigAccess.READ_ONLY);
            var accessor1 = config.ReadValue(10, "SomeInt32");
            var accessor2 = config.ReadValue(1.5, "SomeDouble");

            var actual = config.ToString();
            var expected = @"" + Global.NL;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(10, accessor1.Value);
            Assert.AreEqual(1.5, accessor2.Value);
        }

        [Test()]
        public void OpenConfigAsReadOnlyAndModify_AccessTest()
        {
            var config = Configurable.CreateConfig("OpenConfigAsReadOnlyAndModify_AccessTest", "Access", ConfigAccess.READ_ONLY);
            var accessor1 = config.ReadValue(10, "SomeInt32");
            var accessor2 = config.ReadValue(1.5, "SomeDouble");

            accessor1.Value = 100;
            accessor2.Commentary = "Hello";

            var actual = config.ToString();
            var expected = @"" + Global.NL;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(100, accessor1.Value);
            Assert.AreEqual(1.5, accessor2.Value);
            Assert.AreEqual("Hello", accessor2.Commentary);
        }

        [Test()]
        public void OpenConfigFromStream_Test()
        {
            var configData = @"SomeInt32 =10
SomeDouble =1.5" + Global.NL;
            var stream = Encoding.UTF8.GetBytes(configData).ToMemoryStream();
            var config = Configurable.CreateConfig(stream);

            Assert.AreEqual(10, config.ReadValue(999, "SomeInt32").Value);
            Assert.AreEqual(1.5, config.ReadValue(999D, "SomeDouble").Value);
        }

        [Test()]
        public void CreateConfigFromStream_Test()
        {
            var stream = File.Open(Path.GetTempFileName(), FileMode.Open);
            var config = Configurable.CreateConfig(stream).Clear();
            config.ReadValue(10, "SomeInt32");
            config.ReadValue(1.3, "SomeDouble");

            var actual = config.ToString();
            var expected = @"SomeInt32 =10
SomeDouble =1.3" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void CreateConfig_WithTwoAccesorsTest()
        {
            var configA = Configurable.CreateConfig("Test", "SomeDir", "SectionA").Clear();
            var configB = Configurable.CreateConfig("Test", "SomeDir", "SectionB").Clear();

            configA.ReadValue("A", "SomeString");
            configB.ReadValue("B", "SomeString");

            var actualA = configA.ToString();
            var expectedA = @"[SectionA]
SomeString =#'A'" + Global.NL;

            Assert.AreEqual(expectedA, actualA);

            var actualB = configB.ToString();
            var expectedB = @"[SectionB]
SomeString =#'B'" + Global.NL;

            Assert.AreEqual(expectedB, actualB);
        }

        [Test()]
        public void CreateConfig_Test()
        {
            var config = Configurable.CreateConfig("Test", "SomeDir").Clear();
            config.ReadValue(10, "SomeInt32");
            config.ReadValue(1.3, "SomeDouble");

            var actual = config.ToString();
            var expected = @"SomeInt32 =10
SomeDouble =1.3" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void CreateConfig_WithCommentaryTest()
        {
            var config = Configurable.CreateConfig("CreateConfig_WithCommentaryTest", "SomeDir").Clear();
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
            var config = Configurable.CreateConfig("CreateConfig_WithMultilineCommentaryTest", "SomeDir").Clear();
            var accessor = config.ReadValue(-1, "SomeInt32").SetComment($"First line - {Global.NL}second");
            config.ReadValue(1.3e2, "SomeDouble");

            var actual = config.ToString();
            var expected = @"SomeInt32 =-1\\First line - second
SomeDouble =130" + Global.NL;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual($"First line - {Global.NL}second", accessor.Commentary);
        }

        [Test()]
        public void CreateConfig_ArrayValueTest()
        {
            var config = Configurable.CreateConfig("CreateConfig_ArrayValueTest").Clear();
            config.ReadValue(10, "SomeInt32");
            config.ReadValue(new double[] { -1, 2, 2.345 }, "SomeDoubleArr");
            config.ReadValue(new double[] { }, "SomeEmptyDoubleArr");

            var actual = config.ToString();
            var expected = @"SomeInt32 =10
SomeDoubleArr =-1 2 2.345
SomeEmptyDoubleArr =" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void CreateConfig_NullableValueTypesSupportTest()
        {
            var config = Configurable.CreateConfig("CreateConfig_NullableValueTypesSupportTest").Clear();
            Assert.Throws<NotSupportedException>(() => config.ReadValue<int?>(null, "SomeInt32"));
            Assert.Throws<NotSupportedException>(() => config.ReadValue(new double?[] { null, -1, 2, 2.345, null }, "SomeDoubleArr"));
        }

        [Test()]
        public void CreateAndModify_Test()
        {
            var config = Configurable.CreateConfig("CreateAndModify", "").Clear();
            config.ReadValue(10, "SomeInt32");
            var accessor = config.ReadValue("Hello ", "SomeString");
            config.ReadValue(3, "SomeByte");

            accessor.Value = $"Hello {Global.Random.NextENWord()}";
        }

        [Test()]
        public void CreateAndModify_WithCommentaryTest()
        {
            var config = Configurable.CreateConfig("CreateAndModify_WithCommentaryTest", "SomeDir").Clear();
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
            var config = Configurable.CreateConfig("CreateAndModify_RemoveCommentaryTest", "SomeDir").Clear();
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
            var config = Configurable.CreateConfig("CreateAndModifyArray").Clear();
            var accessor = config.ReadValue(new[] { 10, 20, 55 }, "SomeInt32Array");

            var expected = Global.Random.NextUnique(0, 1000, 10).ToArray();
            accessor.Value = expected;
            var actual = accessor.Value;

            Assert.AreEqual(expected, actual);
        }

        public enum ABC
        {
            A, B, C
        }
        [Test()]
        public void CreateAndModifyEnum_Test()
        {
            var config = Configurable.CreateConfig("CreateAndModifyEnum_Test").Clear();
            var accessor1 = config.ReadValue(new[] { ABC.A, ABC.B, ABC.C }, "EnumArr");
            var accessor2 = config.ReadValue(ABC.B, "Enum");

            {
                var actual1 = config.ToString();
                var expected1 = @"EnumArr =A B C
Enum =B" + Global.NL;

                Assert.AreEqual(expected1, actual1);
            }

            {
                accessor1.Value[1] = ABC.C;
                accessor2.Value = ABC.C;
                accessor1.Commentary = "it is array";
                accessor2.Commentary = "it is single value";

                var actual2 = config.ToString();
                var expected2 = @"EnumArr =A C C\\it is array
Enum =C\\it is single value" + Global.NL;

                Assert.AreEqual(expected2, actual2);
            }
        }

        [Test()]
        public void CreateTwoFieldsWithSameName_Test()
        {
            var config = Configurable.CreateConfig("CreateTwoFieldsWithSameName", "").Clear();
            config.ReadValue("ABC", "SomeValue");
            config.ReadValue(123, "SomeValue");

            var actual = config.ToString();
            var expected = "SomeValue =#'ABC'" + Global.NL;

            Assert.AreEqual(expected, actual);
        }
    }
}