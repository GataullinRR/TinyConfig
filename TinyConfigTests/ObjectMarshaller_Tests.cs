using NUnit.Framework;
using TinyConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;
using TinyConfig.Marshallers;
using Vectors;
using System.Runtime.CompilerServices;

namespace TinyConfig.Tests
{
    [TestFixture()]
    public class ObjectMarshaller_IntegrationalTests
    {
        [Test()]
        public void TryPack_FlatStructure()
        {
            var config = (ConfigAccessor)Configurable.CreateConfig("TryPack_FlatStructure", "ObjectMarshaller_IntegrationalTests").Clear();
            var proxy = new Proxy(config, "Object1");

            var m = new FlatStructObjectMarshaller();
            Assert.True(m.TryPack(proxy, new V2(1, 2)));

            var actual = config.ToString();
            var expected = @"[Object1]
X=1
Y=2
";

            Assert.AreEqual(expected, actual);
        }

    }
}