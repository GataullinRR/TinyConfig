using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyConfig;
using Utilities;
using Utilities.Extensions;
using Utilities.Types;

namespace TinyConfig.Tests
{
    [TestFixture()]
    class SectionsTreeBuilder_IntegrationalTests
    {
        [Test()]
        public void BuildTree_OnlyGoodSections()
        {
            string data =
@"
[S1]
[S1.S1]
[S1.S2]
[S1.S2.S1]
[S1.S2.S2]
[S2]
[S2.S1]
[S3]";

            var S1S2S1 = new SectionsTreeBuilder.RootSection(
                new Section("S1.S2.S1"),
                new SectionsTreeBuilder.RootSection[0],
                new string[] { "[S1.S2.S1]" }
                );
            var S1S2S2 = new SectionsTreeBuilder.RootSection(
                new Section("S1.S2.S2"),
                new SectionsTreeBuilder.RootSection[0],
                new string[] { "[S1.S2.S2]" }
                );
            var S1S1 = new SectionsTreeBuilder.RootSection(
                 new Section("S1.S1"),
                 new SectionsTreeBuilder.RootSection[0],
                 new string[] { "[S1.S1]" }
                );
            var S1S2 = new SectionsTreeBuilder.RootSection(
                 new Section("S1.S2"),
                 new[] { S1S2S1, S1S2S2 },
                 new Enumerable<string> { "[S1.S2]", S1S2S1.Lines, S1S2S2.Lines }
                 );
            var S2S1 = new SectionsTreeBuilder.RootSection(
                 new Section("S2.S1"),
                 new SectionsTreeBuilder.RootSection[0],
                 new string[] { "[S2.S1]" }
                 );
            var S1 = new SectionsTreeBuilder.RootSection(
                 new Section("S1"),
                 new[] { S1S1, S1S2 },
                 new Enumerable<string> { "[S1]", S1S1.Lines, S1S2.Lines }
                 );
            var S2 = new SectionsTreeBuilder.RootSection(
                 new Section("S2"),
                 new[] { S2S1 },
                 new Enumerable<string> { "[S2]", S2S1.Lines }
                 );
            var S3 = new SectionsTreeBuilder.RootSection(
                 new Section("S3"),
                 new SectionsTreeBuilder.RootSection[0],
                 new string[] { "[S3]" }
                 );
            var ROOT = new SectionsTreeBuilder.RootSection(
                 new Section(null),
                 new[] { S1, S2, S3 },
                 new Enumerable<string> { S1.Lines, S2.Lines, S3.Lines }
                 );
            var expectedAllChildren = new[]
            {
                ROOT,
                S1,
                S1S1,
                S1S2,
                S1S2S1,
                S1S2S2,
                S2,
                S2S1,
                S3
            };

            var tree = SectionsTreeBuilder.BuildTree(SectionsFinder.GetSections(data.Split(Global.NL)).ToArray());

            Assert.AreEqual(expectedAllChildren, tree.AllChildren);
        }
    }
}
