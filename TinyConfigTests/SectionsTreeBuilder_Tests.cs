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

            var S1S2S1 = new SectionsTreeBuilder.RootSection()
            {
                Section = new Section("S1.S2.S1"),
                Lines = new string[] { "[S1.S2.S1]" },
                Children = new SectionsTreeBuilder.RootSection[0]
            };
            var S1S2S2 = new SectionsTreeBuilder.RootSection()
            {
                Section = new Section("S1.S2.S2"),
                Lines = new string[] { "[S1.S2.S2]" },
                Children = new SectionsTreeBuilder.RootSection[0]
            };
            var S1S1 = new SectionsTreeBuilder.RootSection()
            {
                Section = new Section("S1.S1"),
                Lines = new string[] { "[S1.S1]" },
                Children = new SectionsTreeBuilder.RootSection[0]
            };
            var S1S2 = new SectionsTreeBuilder.RootSection()
            {
                Section = new Section("S1.S2"),
                Lines = new Enumerable<string> { "[S1.S2]", S1S2S1.Lines, S1S2S2.Lines },
                Children = new [] { S1S2S1, S1S2S2 },
            };
            var S2S1 = new SectionsTreeBuilder.RootSection()
            {
                Section = new Section("S2.S1"),
                Lines = new string[] { "[S2.S1]" },
                Children = new SectionsTreeBuilder.RootSection[0]
            };
            var S1 = new SectionsTreeBuilder.RootSection()
            {
                Section = new Section("S1"),
                Lines = new Enumerable<string> { "[S1]", S1S1.Lines, S1S2.Lines },
                Children = new[] { S1S1, S1S2 },
            };
            var S2 = new SectionsTreeBuilder.RootSection()
            {
                Section = new Section("S2"),
                Lines = new Enumerable<string> { "[S2]", S2S1.Lines },
                Children = new[] { S2S1 },
            };
            var S3 = new SectionsTreeBuilder.RootSection()
            {
                Section = new Section("S3"),
                Lines = new string[] { "[S3]" },
                Children = new SectionsTreeBuilder.RootSection[0]
            };
            var ROOT = new SectionsTreeBuilder.RootSection()
            {
                Section = new Section(null),
                Lines = new Enumerable<string> { "", S1.Lines, S2.Lines, S3.Lines },
                Children = new[] { S1, S2, S3 },
            };
            var expectedAllChildren = new[]
            {
                S1,
                S1S1,
                S1S2,
                S1S2S1,
                S1S2S2,
                S2,
                S2S1,
                S3
            };

            var tree = new SectionsTreeBuilder().BuildTree(SectionsFinder.GetSections(data.Split(Global.NL)).ToArray());

            Assert.AreEqual(expectedAllChildren, tree.AllChildren);
        }
    }
}
