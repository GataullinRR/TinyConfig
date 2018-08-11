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
            //var actualLocations = sections.Select(s => s.SectionLocation);

            var expectedSections = new[] { null, "S1", "S1.S1", "S1.S2", "S1.S2.S1", "S1.S2.S2", "S2", "S2.S1", "S3" }.Select(sn => new Section(sn));

            //var expectedLocations = new[] { "0:0", "1:1", "2:2", "3:3", "4:4", "5:5", "6:6", "7:7", "8:8" }.Select(sn => Interval.Parse(sn).AsIntInterval);

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
            //Assert.AreEqual(expectedLocations, actualLocations);
        }
    }
}