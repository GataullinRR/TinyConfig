﻿using NUnit.Framework;
using TinyConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;
using System.IO;
using Vectors;

namespace TinyConfig.Tests
{
    [TestFixture()]
    public class System_Tests
    {
        [Test()]
        public void RemoveValues()
        {
            var config = Configurable.CreateConfig("RemoveValues").Clear();
            var value1 = config.ReadValue(1, "Int1");
            var value2 = config.ReadValue(2, "Int2");
            var value3 = config.ReadValue(3, "Int3");

            value1.Remove();
            value3.Remove();

            var actual = config.ToString();
            var expected = @"Int2=2" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void RemoveObjects()
        {
            var config = Configurable.CreateConfig("RemoveObjects").Clear();
            var value1 = config.ReadObject(new V2(1, 2), "V1");
            var value2 = config.ReadObject(new V2(3, 4), "V2");
            var value3 = config.ReadObject(new V2(5, 6), "V3");

            value1.Remove();
            value3.Remove();

            var actual = config.ToString();
            var expected = @"[V2]
X=3
Y=4" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void OpenSameConfigAsReadOnly()
        {
            var config1 = Configurable.CreateConfig("OpenSameConfigAsReadOnly", "Access", ConfigAccess.READ_ONLY);
            var config2 = Configurable.CreateConfig("OpenSameConfigAsReadOnly", "Access", ConfigAccess.READ_ONLY);
            var accessor1 = config1.ReadValue(1, "Key");
            var accessor2 = config2.ReadValue(2, "Key");

            var actual1 = config1.ToString();
            var expected1 = @"" + Global.NL;
            var actual2 = config2.ToString();
            var expected2 = @"" + Global.NL;

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual(1, accessor1.Value);
            Assert.AreEqual(2, accessor2.Value);
        }

        [Test()]
        public void OpenSameConfigAsReadWrite()
        {
            Configurable.CreateConfig("OpenSameConfigAsReadWrite", "Access", ConfigAccess.READ_WRITE);
            Assert.Throws<InvalidOperationException>(() =>
                Configurable.CreateConfig("OpenSameConfigAsReadWrite", "Access", ConfigAccess.READ_WRITE));
        }

        [Test()]
        public void OpenAlreadyOpenedAsReadOnlyConfigAsReadWrite()
        {
            Configurable.CreateConfig("OpenAlreadyOpenedAsReadOnlyConfigAsReadWrite", "Access", ConfigAccess.READ_ONLY);
            Configurable.CreateConfig("OpenAlreadyOpenedAsReadOnlyConfigAsReadWrite", "Access", ConfigAccess.READ_ONLY);
            Assert.Throws<InvalidOperationException>(() => 
                Configurable.CreateConfig("OpenAlreadyOpenedAsReadOnlyConfigAsReadWrite", "Access", ConfigAccess.READ_WRITE));
        }

        [Test()]
        public void ReadFromEmptyReadOnlyConfig()
        {
            var config = Configurable.CreateConfig("ReadFromEmptyReadOnlyConfig", "Access", ConfigAccess.READ_ONLY);
            var accessor1 = config.ReadValue(10, "SomeInt32");
            var accessor2 = config.ReadValue(1.5, "SomeDouble");

            var actual = config.ToString();
            var expected = @"" + Global.NL;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(10, accessor1.Value);
            Assert.AreEqual(1.5, accessor2.Value);
        }

        [Test()]
        public void ModifyingValueInReadOnlyConfig()
        {
            var config = Configurable.CreateConfig("ModifyingValueInReadOnlyConfig", "Access", ConfigAccess.READ_ONLY);
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
        public void ReadConfigFromStream()
        {
            var configData = @"SomeInt32 =10
SomeDouble =1.5" + Global.NL;
            var stream = Encoding.UTF8.GetBytes(configData).ToMemoryStream();
            var config = Configurable.CreateConfig(stream);

            Assert.AreEqual(10, config.ReadValue(999, "SomeInt32").Value);
            Assert.AreEqual(1.5, config.ReadValue(999D, "SomeDouble").Value);
        }

        [Test()]
        public void WriteToCreatedFromStreamConfig()
        {
            var stream = File.Open(Path.GetTempFileName(), FileMode.Open);
            var config = Configurable.CreateConfig(stream).Clear();
            config.ReadValue(10, "SomeInt32");
            config.ReadValue(1.3, "SomeDouble");

            var actual = config.ToString();
            var expected = @"SomeInt32=10
SomeDouble=1.3" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ReleaseFile()
        {
            var config = Configurable.CreateConfig("ReleaseFile").Clear();

            Assert.Throws<IOException>(() => new FileStream(config.SourceInfo.FilePath, FileMode.Open));
            Configurable.ReleaseFile(config.SourceInfo.FilePath);
            Assert.DoesNotThrow(() => new FileStream(config.SourceInfo.FilePath, FileMode.Open));
        }

        [Test()]
        public void WriteValueAfterFileReleased()
        {
            var config = Configurable.CreateConfig("WriteValueAfterFileReleased").Clear();
            Configurable.ReleaseFile(config.SourceInfo.FilePath);
            Assert.Throws<ObjectDisposedException>(() => config.ReadValue("1", "Key1"));
        }

        [Test()]
        public void WriteToConfigWithTwoSections()
        {
            var configA = Configurable.CreateConfig("WriteToConfigWithTwoSections", "SomeDir", "SectionA").Clear();
            var configB = Configurable.CreateConfig("WriteToConfigWithTwoSections", "SomeDir", "SectionB").Clear();

            configA.ReadValue("A", "SomeString");
            configB.ReadValue("B", "SomeString");

            var actualA = configA.ToString();
            var expectedA = @"[SectionA]
SomeString=#'A'" + Global.NL;

            Assert.AreEqual(expectedA, actualA);

            var actualB = configB.ToString();
            var expectedB = @"[SectionB]
SomeString=#'B'" + Global.NL;

            Assert.AreEqual(expectedB, actualB);
        }

        [Test()]
        public void WriteToSubsection()
        {
            var config = Configurable.CreateConfig("WriteToSubsection").Clear();
            config.ReadValueFrom("A", null, "SomeString");
            config.ReadValueFrom("B", "Section1", "SomeString");
            config.ReadValueFrom("C", "Section1.Subsection1", "SomeString");

            var actual = config.ToString();
            var expected = @"SomeString=#'A'
[Section1]
SomeString=#'B'
[Section1.Subsection1]
SomeString=#'C'
";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void WriteСonsecutivelyToSingleSubsection()
        {
            var config = Configurable.CreateConfig("WriteСonsecutivelyToSingleSubsection").Clear();
            config.ReadValueFrom("A", "Section1", "SomeString1");
            config.ReadValueFrom("B", "Section1", "SomeString2");

            var actual = config.ToString();
            var expected = @"[Section1]
SomeString1=#'A'
SomeString2=#'B'
";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ModifyValueInSubsection()
        {
            var config = Configurable.CreateConfig("ModifyValueInSubsection").Clear();
            var a = config.ReadValueFrom("A", null, "SomeString");
            var b = config.ReadValueFrom("B", "Section1", "SomeString");
            var c = config.ReadValueFrom("C", "Section1.Subsection1", "SomeString");
            c.Value = "NewC";
            a.Value = "NewA";
            b.Value = "NewB";

            var actual = config.ToString();
            var expected = @"SomeString=#'NewA'
[Section1]
SomeString=#'NewB'
[Section1.Subsection1]
SomeString=#'NewC'
";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ReadFromSubsectionOfNotEmptyConfig()
        {
            var config = Configurable.CreateConfig("ReadFromSubsectionOfNotEmptyConfig").Clear();
            config.ReadValueFrom("A", null, "SomeString");
            config.ReadValueFrom("B", "Section1", "SomeString");
            config.ReadValueFrom("C", "Section1.Subsection1", "SomeString");
            config.Close();
            Configurable.ReleaseFile(config.SourceInfo.FilePath);

            config = Configurable.CreateConfig("ReadFromSubsectionOfNotEmptyConfig");
            var actual = config.ToString();
            var expected = @"SomeString=#'A'
[Section1]
SomeString=#'B'
[Section1.Subsection1]
SomeString=#'C'
";

            Assert.AreEqual(expected, actual);
            Assert.AreEqual("A", config.ReadValueFrom("", null, "SomeString").Value);
            Assert.AreEqual("B", config.ReadValueFrom("", "Section1", "SomeString").Value);
            Assert.AreEqual("C", config.ReadValueFrom("", "Section1.Subsection1", "SomeString").Value);
        }

        [Test()]
        public void WriteValue()
        {
            var config = Configurable.CreateConfig("WriteValue", "SomeDir").Clear();
            config.ReadValue(10, "SomeInt32");
            config.ReadValue(1.3, "SomeDouble");

            var actual = config.ToString();
            var expected = @"SomeInt32=10
SomeDouble=1.3" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void WriteMixedTypesThroughRead()
        {
            var config = Configurable.CreateConfig("WriteMixedTypesThroughRead").Clear();
            config.Read(0, "Int32");
            config.Read(new V2(1, 2), "V2");
            config.Read(3.3, "Double");

            var actual = config.ToString();
            var expected = @"Int32=0
Double=3.3
[V2]
X=1
Y=2" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void WriteCommentary()
        {
            var config = Configurable.CreateConfig("WriteCommentary", "SomeDir").Clear();
            var accessor = config.ReadValue(10, "SomeInt32").SetComment("Hello!");
            config.ReadValue(1.3, "SomeDouble");

            var actual = config.ToString();
            var expected = @"SomeInt32=10\\Hello!
SomeDouble=1.3" + Global.NL;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual("Hello!", accessor.Commentary);
        }

        [Test()]
        public void WriteMultilineCommentary()
        {
            var config = Configurable.CreateConfig("WriteMultilineCommentary", "SomeDir").Clear();
            var accessor = config.ReadValue(-1, "SomeInt32").SetComment($"First line - {Global.NL}second");
            config.ReadValue(1.3e2, "SomeDouble");

            var actual = config.ToString();
            var expected = @"SomeInt32=-1\\First line - second
SomeDouble=130" + Global.NL;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual($"First line - {Global.NL}second", accessor.Commentary);
        }

        [Test()]
        public void WriteArrayValue()
        {
            var config = Configurable.CreateConfig("WriteArrayValue").Clear();
            config.ReadValue(10, "SomeInt32");
            config.ReadValue(new double[] { -1, 2, 2.345 }, "SomeDoubleArr");
            config.ReadValue(new double[] { }, "SomeEmptyDoubleArr");

            var actual = config.ToString();
            var expected = @"SomeInt32=10
SomeDoubleArr=-1 2 2.345
SomeEmptyDoubleArr=" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void WriteFlatStructObjects()
        {
            var config = Configurable.CreateConfig("WriteFlatStructObjects").Clear();
            config.ReadObject(new V2(1, 2), "Object1");
            config.ReadObject(new V2(3, 4), "Object2");

            var actual = config.ToString();
            var expected = @"[Object1]
X=1
Y=2
[Object2]
X=3
Y=4" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void WriteFlatStructObjectsToSubsections()
        {
            var config = Configurable.CreateConfig("WriteFlatStructObjectsToSubsections").Clear();
            config.ReadObjectFrom(new V2(7, 8), "ROOT.SECTION", "Object4");
            config.ReadObjectFrom(new V2(3, 4), "ROOT", "Object2");
            config.ReadObjectFrom(new V2(1, 2), null, "Object1");
            config.ReadObjectFrom(new V2(5, 6), "ROOT", "Object3");

            var actual = config.ToString();
            var expected = @"[Object1]
X=1
Y=2
[ROOT]
[ROOT.Object2]
X=3
Y=4
[ROOT.Object3]
X=5
Y=6
[ROOT.SECTION]
[ROOT.SECTION.Object4]
X=7
Y=8" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Serializable]
        struct MixedFlatnessStruct
        {
            double X1;
            V2 V1;

            public MixedFlatnessStruct(double x1, V2 v1)
            {
                X1 = x1;
                V1 = v1;
            }
        }

        [Test()]
        public void WriteMixedFlatnessStructObject()
        {
            var config = Configurable.CreateConfig("WriteMixedFlatnessStructObject").Clear();
            config.ReadObject(new MixedFlatnessStruct(1, new V2(2, 3)), "Object1");
            config.ReadObject(new MixedFlatnessStruct(4, new V2(5, 6)), "Object2");

            var actual = config.ToString();
            var expected = @"[Object1]
X1=1
[Object1.V1]
X=2
Y=3
[Object2]
X1=4
[Object2.V1]
X=5
Y=6" + Global.NL;

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ModifyValue()
        {
            var config = Configurable.CreateConfig("ModifyValue", "").Clear();
            config.ReadValue(10, "SomeInt32");
            var accessor = config.ReadValue("Hello ", "SomeString");
            config.ReadValue(3, "SomeByte");

            accessor.Value = $"Hello {Global.Random.NextENWord()}";
        }

        [Test()]
        public void FlushAfterValueModifying()
        {
            var config = Configurable.CreateConfig("FlushAfterValueModifying").Clear();
            var initial = new FileInfo(config.SourceInfo.FilePath).Length;

            config.ReadValue(10, "SomeInt32");
            var afterRead1 = new FileInfo(config.SourceInfo.FilePath).Length;

            var accessor = config.ReadValue("Hello ", "SomeString");
            var afterRead2 = new FileInfo(config.SourceInfo.FilePath).Length;

            accessor.Value = "------------------------------------";
            var afterModify = new FileInfo(config.SourceInfo.FilePath).Length;

            Assert.AreEqual(Encoding.UTF8.GetPreamble().Length, initial);
            Assert.AreNotEqual(initial, afterRead1);
            Assert.AreNotEqual(initial, afterRead2);
            Assert.True(afterRead2 > afterRead1);
            Assert.AreNotEqual(initial, afterModify);
            Assert.True(afterModify > afterRead2);
        }

        [Test()]
        public void ReadAndModifyCommentary()
        {
            var config = Configurable.CreateConfig("ReadAndModifyCommentary", "SomeDir").Clear();
            config.ReadValue((byte)3, "SomeUInt8");
            var accessor = config.ReadValue(new int[] { 1, 2, 3 }, "SomeInt32Arr").SetComment("Hello!");
            config.ReadValue("Hello", "SomeString");

            var actual1 = config.ToString();
            var expected1 = @"SomeUInt8=3
SomeInt32Arr=1 2 3\\Hello!
SomeString=#'Hello'" + Global.NL;

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual("Hello!", accessor.Commentary);

            accessor.SetComment("It is int[]");
            var actual2 = config.ToString();
            var expected2 = @"SomeUInt8=3
SomeInt32Arr=1 2 3\\It is int[]
SomeString=#'Hello'" + Global.NL;

            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual("It is int[]", accessor.Commentary);
        }

        [Test()]
        public void ReadAndEraseCommentary()
        {
            var config = Configurable.CreateConfig("ReadAndEraseCommentary", "SomeDir").Clear();
            config.ReadValue((byte)3, "SomeUInt8").SetComment("Hi!");
            config.ReadValue(new int[] { 1, 2, 3 }, "SomeInt32Arr");
            var accessor = config.ReadValue("Hello", "SomeString").SetComment("Hello!");

            var actual1 = config.ToString();
            var expected1 = @"SomeUInt8=3\\Hi!
SomeInt32Arr=1 2 3
SomeString=#'Hello'\\Hello!" + Global.NL;

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual("Hello!", accessor.Commentary);

            accessor.SetComment(null);
            var actual2 = config.ToString();
            var expected2 = @"SomeUInt8=3\\Hi!
SomeInt32Arr=1 2 3
SomeString=#'Hello'" + Global.NL;

            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual(null, accessor.Commentary);
        }

        [Test()]
        public void ModifyArrayValue()
        {
            var config = Configurable.CreateConfig("ModifyArrayValue").Clear();
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
        public void ReadWriteModifyEnum()
        {
            var config = Configurable.CreateConfig("ReadWriteModifyEnum").Clear();
            var accessor1 = config.ReadValue(new[] { ABC.A, ABC.B, ABC.C }, "EnumArr");
            var accessor2 = config.ReadValue(ABC.B, "Enum");

            {
                var actual1 = config.ToString();
                var expected1 = @"EnumArr=A B C
Enum=B" + Global.NL;

                Assert.AreEqual(expected1, actual1);
            }

            {
                accessor1.Value[1] = ABC.C;
                accessor2.Value = ABC.C;
                accessor1.Commentary = "it is array";
                accessor2.Commentary = "it is single value";

                var actual2 = config.ToString();
                var expected2 = @"EnumArr=A C C\\it is array
Enum=C\\it is single value" + Global.NL;

                Assert.AreEqual(expected2, actual2);
            }
        }

        [Test()]
        public void CreateTwoFieldsWithSameName()
        {
            var config = Configurable.CreateConfig("CreateTwoFieldsWithSameName").Clear();
            config.ReadValue("ABC", "SomeValue");
            Assert.Throws<InvalidOperationException>(() => config.ReadValue(123, "SomeValue"));
        }

        [Test()]
        public void ModifyAfterClose()
        {
            var config = Configurable.CreateConfig("ModifyAfterClose").Clear();
            config.Close();
            var value = config.ReadValue("1", "Key1");
            value.Value = "2";

            Assert.AreEqual("2", value.Value);
        }

        class VectorMarshaller : ExactValueMarshaller<V2>
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

        class MyIntMarshaller : ExactValueMarshaller<int>
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
        public void AddMarshaller()
        {
            var config = Configurable.CreateConfig("AddMarshaller").Clear()
                .AddMarshaller<VectorMarshaller>();
            config.ReadValue(10, "SomeInt32");
            config.ReadValue(new V2(-9, 9), "SomeV2");

            var actual = config.ToString();
            var expected = @"SomeInt32=10
SomeV2=X:-9 Y:9
";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void OverrideStandardMarshaller()
        {
            var config = Configurable.CreateConfig("OverrideStandardMarshaller").Clear()
                .AddMarshaller<MyIntMarshaller>();
            config.ReadValue(10, "SomeInt32");

            var actual = config.ToString();
            var expected = @"SomeInt32=DEC: 10
";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void WriteNullValues()
        {
            var config = Configurable.CreateConfig("WriteNullValues").Clear();
            config.ReadValue<string>(null, "String");
            config.ReadValue<MemoryStream>(null, "MemoryStream");

            var actual = config.ToString();
            var expected = @"String=NULL
MemoryStream=NULL
";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void WriteArrayOfNullValues()
        {
            var config = Configurable.CreateConfig("WriteArrayOfNullValues").Clear();
            config.ReadValue(
                new MemoryStream[] { null, null, null },
                "MemoryStreamArr");

            var actual = config.ToString();
            var expected = @"MemoryStreamArr=NULL NULL NULL
";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void WriteArrayOfNullAndNotNullValues()
        {
            var config = Configurable.CreateConfig("WriteArrayOfNullAndNotNullValues").Clear();
            config.ReadValue(
                new MemoryStream[] { null, new MemoryStream("Hello1".GetASCIIBytes()), null, null }, 
                "MemoryStreamArr1");
            config.ReadValue(
                new MemoryStream[] { new MemoryStream("Hello2".GetASCIIBytes()), null, null },
                "MemoryStreamArr2");
            var actual = config.ToString();
            var expected = @"MemoryStreamArr1=NULL True-SGVsbG8x NULL NULL
MemoryStreamArr2=True-SGVsbG8y NULL NULL
";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ReadNullValues()
        {
            var config = Configurable.CreateConfig("ReadNullValues").Clear();
            config.ReadValue<string>(null, "String");
            config.ReadValue<MemoryStream>(null, "MemoryStream");
            config.Close();
            Configurable.ReleaseFile(config.SourceInfo.FilePath);

            config = Configurable.CreateConfig("ReadNullValues");

            Assert.AreEqual(null, config.ReadValue("", "String").Value);
            Assert.AreEqual(null, config.ReadValue(new MemoryStream(), "MemoryStream").Value);
        }

        [Test()]
        public void WriteNullableValueTypes()
        {
            var config = Configurable.CreateConfig("WriteNullableValueTypes").Clear();
            Assert.Throws<NotSupportedException>(() => config.ReadValue<int?>(null, "SomeInt32"));
            Assert.Throws<NotSupportedException>(() => config.ReadValue(new double?[] { null, -1, 2, 2.345, null }, "SomeDoubleArr"));
        }

        [Test()]
        public void ReadFromNotEmptyConfig()
        {
            var config = Configurable.CreateConfig("ReadFromNotEmptyConfig").Clear();
            config.ReadValue("1", "Key1");
            config.ReadValue("2", "Key2");
            config.Close();
            Configurable.ReleaseFile(config.SourceInfo.FilePath);

            config = Configurable.CreateConfig("ReadFromNotEmptyConfig");
            Assert.AreEqual("1", config.ReadValue("", "Key1").Value);
            Assert.AreEqual("2", config.ReadValue("", "Key2").Value);
        }

        [Test()]
        public void ReadFlatStructObjectsFromNotEmptyConfig()
        {
            var config = Configurable.CreateConfig("ReadFlatStructObjectsFromNotEmptyConfig").Clear();
            config.ReadObject(new V2(1, 2), "Object1");
            config.ReadObject(new V2(3, 4), "Object2");
            config.Close();
            Configurable.ReleaseFile(config.SourceInfo.FilePath);

            config = Configurable.CreateConfig("ReadFlatStructObjectsFromNotEmptyConfig");
            Assert.AreEqual(new V2(1, 2), config.ReadObject(V2.Zero, "Object1").Value);
            Assert.AreEqual(new V2(3, 4), config.ReadObject(V2.Zero, "Object2").Value);
        }

        [Test()]
        public void ReadAfterClose()
        {
            var config = Configurable.CreateConfig("ReadAfterClose").Clear();
            var written = config.ReadValue("1", "Key1");
            config.Close();
            Configurable.ReleaseFile(config.SourceInfo.FilePath);

            config = Configurable.CreateConfig("ReadAfterClose");
            config.Close();
            var read = config.ReadValue("", "Key1");

            Assert.AreEqual(written.Value, read.Value);
        }

        [Test()]
        public void ReadArray()
        {
            var stream = "Arr=1 2 3".GetBytes(Encoding.UTF8).ToMemoryStream();
            var config = Configurable.CreateConfig(stream);

            Assert.AreEqual(new[] { 1, 2, 3 }, config.ReadValue(new int[0], "Arr").Value);
        }

        [Test()]
        public void WriteValueAfterClose()
        {
            var config = Configurable.CreateConfig("ReadValue_WriteValueAfterClose").Clear();
            config.Close();
            var value = config.ReadValue("1", "Key1");

            Assert.AreEqual("1", value.Value);
        }
    }
}