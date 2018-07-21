using NUnit.Framework;
using TinyConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Utilities.Extensions;
using Utilities;

namespace TinyConfig.Tests
{
    [TestFixture()]
    public class KVPExtractor_Tests
    {
        [Test()]
        public void ExtractAll_OneLineKVPsOnlyTest()
        {
            var data = @"KEY1 =Value
fff=2";

            var actual = KVPExtractor.ExtractAll(new StreamReader(new MemoryStream(data.GetBytes(Encoding.UTF8))));
            var expected = new[]
            {
                new ConfigKVP ("KEY1", new ConfigValue("Value", false), null),
                new ConfigKVP ("fff", new ConfigValue("2", false), null),
            };

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ExtractAll_MultilineKVPsOnlyTest()
        {
            var data = @"KEY1 = #'Value ''line1''
line2
l''i''ne3'
KEY2= # 'Hello ''friend''!'
KEY3=#'''Hel''''lo '''''";

            var actual = KVPExtractor.ExtractAll(new StreamReader(new MemoryStream(data.GetBytes(Encoding.UTF8))));
            var expected = new[]
            {
                new ConfigKVP("KEY1", new ConfigValue("Value 'line1'" + Global.NL + "line2" + Global.NL + "l'i'ne3", true), null),
                new ConfigKVP ("KEY2", new ConfigValue("Hello 'friend'!", true), null),
                new ConfigKVP ("KEY3", new ConfigValue("'Hel''lo ''", true), null),
            };

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ExtractAll_MixedKVPsTest()
        {
            var data = @"KEY1 = Value
KEY2= # 'Hello ''friend''!'
KEY3= eeHel''''lo";

            var actual = KVPExtractor.ExtractAll(new StreamReader(new MemoryStream(data.GetBytes(Encoding.UTF8))));
            var expected = new[]
            {
                new ConfigKVP ("KEY1", new ConfigValue(" Value", false), null),
                new ConfigKVP("KEY2", new ConfigValue("Hello 'friend'!", true), null),
                new ConfigKVP("KEY3", new ConfigValue(" eeHel''''lo", false), null),
            };

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ExtractAll_MixedKVPsAndCommentaryTest1()
        {
            var data = @"SomeUInt8 =3
SomeInt32Arr =1 2 3\\It is int[]
SomeString =#'Hello'
";

            var actual = KVPExtractor.ExtractAll(new StreamReader(new MemoryStream(data.GetBytes(Encoding.UTF8))));
            var expected = new[]
            {
                new ConfigKVP("SomeUInt8", new ConfigValue("3", false), null),
                new ConfigKVP("SomeInt32Arr", new ConfigValue("1 2 3", false), "It is int[]"),
                new ConfigKVP("SomeString", new ConfigValue("Hello", true), null),
            };

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ExtractAll_MixedKVPsAndCommentaryTest2()
        {
            var data = @"KEY1 = Value \\comment1
KEY2= # 'Hello ''friend''!' \\ comment2 
KEY3= eeHel''''lo
KEY4=123";

            var actual = KVPExtractor.ExtractAll(new StreamReader(new MemoryStream(data.GetBytes(Encoding.UTF8))));
            var expected = new[]
            {
                new ConfigKVP ("KEY1", new ConfigValue(" Value ", false), "comment1"),
                new ConfigKVP("KEY2", new ConfigValue("Hello 'friend'!", true), " comment2 "),
                new ConfigKVP("KEY3", new ConfigValue(" eeHel''''lo", false), null),
                new ConfigKVP("KEY4", new ConfigValue("123", false), null),
            };

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ExtractAll_CommentaryInValueTest()
        {
            var data = @"KEY1 =# 'Value \\bad comment' \\good comment1
KEY2= # '\\bad commentHello ''friend''!' \\ good comment2
KEY3=(value begin)\\bad comment(value end) \\ good comment3";

            var actual = KVPExtractor.ExtractAll(new StreamReader(new MemoryStream(data.GetBytes(Encoding.UTF8))));
            var expected = new[]
            {
                new ConfigKVP ("KEY1", new ConfigValue(@"Value \\bad comment", true), "good comment1"),
                new ConfigKVP("KEY2", new ConfigValue(@"\\bad commentHello 'friend'!", true), " good comment2"),
                new ConfigKVP("KEY3", new ConfigValue("(value begin)", false), @"bad comment(value end) \\ good comment3"),
            };

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ExtractAll_SectionsTest()
        {
            var data = @"SomeUInt8 =3
[Section1]SomeInt32Arr =1 2 3\\It is int[]
[Section2]
SomeString =#'Hello'[Section3]
[Section4]
SomeVal1 =1
SomeVal2 =2
";

            var actual = KVPExtractor.ExtractAll(new StreamReader(new MemoryStream(data.GetBytes(Encoding.UTF8))));
            var expected = new[]
            {
                new ConfigKVP(new Section(null), "SomeUInt8", new ConfigValue("3", false), null),
                new ConfigKVP(new Section("Section2"), "SomeString", new ConfigValue("Hello", true), null),
                new ConfigKVP(new Section("Section4"), "SomeVal1", new ConfigValue("1", false), null),
                new ConfigKVP(new Section("Section4"), "SomeVal2", new ConfigValue("2", false), null),
            };

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void ExtractAll_IgnoreRepeatingSectionsTest()
        {
            var data = @"[Section1]
SomeUInt8 =3
SomeInt32Arr =1 2 3\\It is int[]
[Section1]
SomeString =#'Hello'[Section3]
[Section2]
SomeVal1 =1
SomeVal2 =2";

            var actual = KVPExtractor.ExtractAll(new StreamReader(new MemoryStream(data.GetBytes(Encoding.UTF8))));
            var expected = new[]
            {
                new ConfigKVP(new Section("Section1"), "SomeUInt8", new ConfigValue("3", false), null),
                new ConfigKVP(new Section("Section1"), "SomeInt32Arr", new ConfigValue("1 2 3", false), "It is int[]"),
                new ConfigKVP(new Section("Section2"), "SomeVal1", new ConfigValue("1", false), null),
                new ConfigKVP(new Section("Section2"), "SomeVal2", new ConfigValue("2", false), null),
            };

            Assert.AreEqual(expected, actual);
        }
    }
}