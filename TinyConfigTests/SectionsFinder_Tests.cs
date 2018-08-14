using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyConfig;
using Utilities;
using Utilities.Extensions;
using Vectors;

namespace TinyConfig.Tests
{
    [TestFixture()]
    class SectionsFinder_Tests
    {
        [Test()]
        public void GetSections()
        {
            string data =
    @"Key=1
[S1]
Key=2
[S1.S1]
Key=3
[S1.S2]
Key=4
[S1.S2.S1]
Key=5
[S2]
Key=6
[S2.S1]
Key=7
[S3]
Key=8";

            var actualSections = SectionsFinder.GetSections(data.Split(Global.NL)).ToArray();

            var expectedSections = new SectionsFinder.SectionInfo[]
            {
                new SectionsFinder.SectionInfo(null, new[] { "Key=1" }, new[] { "Key=1" }, new IntInterval(0, 0), new IntInterval(0, 0)),
                new SectionsFinder.SectionInfo("S1", new[] { "[S1]", "Key=2" }, new[] { "Key=2" }, new IntInterval(1, 2), new IntInterval(2, 2)),
                new SectionsFinder.SectionInfo("S1.S1", new[] { "[S1.S1]", "Key=3"   },  new[] { "Key=3" }, new IntInterval(3, 4), new IntInterval(4, 4)),
                new SectionsFinder.SectionInfo("S1.S2", new[] { "[S1.S2]" , "Key=4" },  new[] { "Key=4" }, new IntInterval(5, 6), new IntInterval(6, 6)),
                new SectionsFinder.SectionInfo("S1.S2.S1", new[] { "[S1.S2.S1]", "Key=5"  },  new[] { "Key=5" }, new IntInterval(7, 8), new IntInterval(8, 8)),
                new SectionsFinder.SectionInfo("S2", new[] { "[S2]" , "Key=6" },  new[] { "Key=6" }, new IntInterval(9, 10), new IntInterval(10, 10)),
                new SectionsFinder.SectionInfo("S2.S1", new[] { "[S2.S1]", "Key=7"  },  new[] { "Key=7" }, new IntInterval(11, 12), new IntInterval(12, 12)),
                new SectionsFinder.SectionInfo("S3", new[] { "[S3]" , "Key=8" },  new[] { "Key=8" }, new IntInterval(13, 14), new IntInterval(14, 14)),
            };

            Assert.AreEqual(expectedSections, actualSections);
        }

        [Test()]
        public void GetSections_OnlyRootSection()
        {
            string data =
    @"Key1=1
Key2=2";

            var actualSections = SectionsFinder.GetSections(data.Split(Global.NL)).ToArray();

            var expectedSections = new SectionsFinder.SectionInfo[]
            {
                new SectionsFinder.SectionInfo(null, new[] { "Key1=1", "Key2=2" }, new[] { "Key1=1", "Key2=2" }, new IntInterval(0, 1), new IntInterval(0, 1)),
            };

            Assert.AreEqual(expectedSections, actualSections);
        }

        [Test()]
        public void GetSections_OnlyBodylessSections()
        {
            string data =
@"[S1]
[S1.S1]
[S1.S2]
[S1.S2.S1]
[S1.S2.S2]
[S2]
[S2.S1]
[S3]";

            var sections = SectionsFinder.GetSections(data.Split(Global.NL)).ToArray();
            var actualSections = sections.Select(s => s.Section);
            var expectedSections = new[] { null, "S1", "S1.S1", "S1.S2", "S1.S2.S1", "S1.S2.S2", "S2", "S2.S1", "S3" }.Select(sn => new Section(sn));

            var dd = new SectionsFinder.SectionInfo[]
            {
                new SectionsFinder.SectionInfo(null, new string[0], new string[0], new IntInterval(-1, -1), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1", new[] { "[S1]" }, new string[0], new IntInterval(0, 0), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S1", new[] { "[S1.S1]" }, new string[0], new IntInterval(1, 1), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S2", new[] { "[S1.S2]" }, new string[0], new IntInterval(2, 2), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S2.S1", new[] { "[S1.S2.S1]" }, new string[0], new IntInterval(3, 3), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S2.S2", new[] { "[S1.S2.S2]" }, new string[0], new IntInterval(4, 4), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S2", new[] { "[S2]" }, new string[0], new IntInterval(5, 5), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S2.S1", new[] { "[S2.S1]" }, new string[0], new IntInterval(6, 6), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S3", new[] { "[S3]" }, new string[0], new IntInterval(7, 7), new IntInterval(-1, -1)),
            };

            Assert.AreEqual(expectedSections, actualSections);
            Assert.AreEqual(dd, sections);
        }

        [Test()]
        public void GetSections_OnlyBodylessSectionsWithWrongHierarchy1()
        {
            string data =
@"[S1]
[S1.S1.WRONG]
[S1.S2]
[S1.S2]
[S1.S2.S2]
[S2]
[S1.S1]
[S3]";

            var actualSections = SectionsFinder.GetSections(data.Split(Global.NL)).ToArray();
            var expectedSections = new SectionsFinder.SectionInfo[]
            {
                new SectionsFinder.SectionInfo(null, new string[0], new string[0], new IntInterval(-1, -1), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1", new[] { "[S1]" }, new string[0], new IntInterval(0, 0), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S2", new[] { "[S1.S2]" }, new string[0], new IntInterval(2, 2), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S2.S2", new[] { "[S1.S2.S2]" }, new string[0], new IntInterval(4, 4), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S2", new[] { "[S2]" }, new string[0], new IntInterval(5, 5), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S3", new[] { "[S3]" }, new string[0], new IntInterval(7, 7), new IntInterval(-1, -1)),
            };

            Assert.AreEqual(expectedSections, actualSections);
        }

        [Test()]
        public void GetSections_OnlyBodylessSectionsWithWrongHierarchy2()
        {
            string data =
@"[S1.WRONG]
[S1]
[S1.S2]
[S1.S2]
[S1.S2.S2.WRONG]
[S1.S2.S2]
[S1.S3]
[S2]
[S3]";

            var actualSections = SectionsFinder.GetSections(data.Split(Global.NL)).ToArray();
            var expectedSections = new SectionsFinder.SectionInfo[]
            {
                new SectionsFinder.SectionInfo(null, new string[0], new string[0], new IntInterval(-1, -1), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1", new[] { "[S1]" }, new string[0], new IntInterval(1, 1), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S2", new[] { "[S1.S2]" }, new string[0], new IntInterval(2, 2), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S2.S2", new[] { "[S1.S2.S2]" }, new string[0], new IntInterval(5, 5), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S3", new[] { "[S1.S3]" }, new string[0], new IntInterval(6, 6), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S2", new[] { "[S2]" }, new string[0], new IntInterval(7), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S3", new[] { "[S3]" }, new string[0], new IntInterval(8), new IntInterval(-1, -1)),
            };

            Assert.AreEqual(expectedSections, actualSections);
        }

        [Test()]
        public void GetSections_EmptyAndNotEmptySections()
        {
            string data =
@"[S1]
[S1.S1]
Key1=1
[S1.S2]
[S1.S2.S1]
[S1.S2.S2]
[S2]
Key2=2
[S2.S1]
[S3]
Key3=3";

            var sections = SectionsFinder.GetSections(data.Split(Global.NL)).ToArray();
            var actualSections = sections.Select(s => s.Section);
            var expectedSections = new[] { null, "S1", "S1.S1", "S1.S2", "S1.S2.S1", "S1.S2.S2", "S2", "S2.S1", "S3" }.Select(sn => new Section(sn));

            var dd = new SectionsFinder.SectionInfo[]
            {
                new SectionsFinder.SectionInfo(null, new string[0], new string[0], new IntInterval(-1, -1), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1", new[] { "[S1]" }, new string[0], new IntInterval(0, 0), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S1", new[] { "[S1.S1]", "Key1=1" }, new [] { "Key1=1" }, new IntInterval(1, 2), new IntInterval(2, 2)),
                new SectionsFinder.SectionInfo("S1.S2", new[] { "[S1.S2]" }, new string[0], new IntInterval(3, 3), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S2.S1", new[] { "[S1.S2.S1]" }, new string[0], new IntInterval(4, 4), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S1.S2.S2", new[] { "[S1.S2.S2]" }, new string[0], new IntInterval(5, 5), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S2", new[] { "[S2]", "Key2=2" },  new [] { "Key2=2" }, new IntInterval(6, 7), new IntInterval(7, 7)),
                new SectionsFinder.SectionInfo("S2.S1", new[] { "[S2.S1]" }, new string[0], new IntInterval(8, 8), new IntInterval(-1, -1)),
                new SectionsFinder.SectionInfo("S3", new[] { "[S3]", "Key3=3" },  new [] { "Key3=3" }, new IntInterval(9, 10), new IntInterval(10, 10)),
            };

            Assert.AreEqual(expectedSections, actualSections);
            Assert.AreEqual(dd, sections);
        }
    }
}