using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyConfig;
using Utilities;
using Utilities.Extensions;

namespace TinyConfigTests
{
    [TestFixture()]
    public class Section_Tests
    {
        [Test()]
        public void ConcatSubsections()
        {
            Assert.AreEqual("Sub1.Sub2.Sub3", Section.ConcatSubsections("Sub1", "Sub2", "Sub3"));
            Assert.AreEqual("Sub1.Sub2", Section.ConcatSubsections("Sub1", "Sub2"));
            Assert.AreEqual("Subsection", Section.ConcatSubsections("Subsection", null));
            Assert.AreEqual("Subsection", Section.ConcatSubsections(null, "Subsection"));
            Assert.AreEqual("", Section.ConcatSubsections(null, null));
        }

        [Test()]
        public void GetParrent()
        {
            Assert.Throws<InvalidOperationException>(() => new Section(null).GetParent());
            Assert.Throws<InvalidOperationException>(() => Section.InvalidSection.GetParent());
            Assert.AreEqual(new Section(null), new Section("Section").GetParent());
            Assert.AreEqual(new Section("Section"), new Section("Section.Sub").GetParent());
            Assert.AreEqual(new Section("Section.Sub1"), new Section("Section.Sub1.Sub2").GetParent());
        }

        [Test()]
        public void Order()
        {
            Assert.AreEqual(0, Section.InvalidSection.Order);
            Assert.AreEqual(0, Section.RootSection.Order);
            Assert.AreEqual(1, new Section("Section").Order);
            Assert.AreEqual(2, new Section("Section.Sub").Order);
        }
    }
}

