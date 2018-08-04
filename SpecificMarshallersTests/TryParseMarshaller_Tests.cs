using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpecificMarshallers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;

namespace SpecificMarshallers.Tests
{
    [TestClass()]
    public class TryParseMarshaller_Tests
    {
        TryParseMarshaller<IPAddress> _marshaller = new TryParseMarshaller<IPAddress>();

        [TestMethod()]
        public void TryPack_Test()
        {
            _marshaller.TryPack(IPAddress.Loopback, out string result);

            Assert.AreEqual("127.0.0.1", result);
        }

        [TestMethod()]
        public void TryUnpack_Test()
        {
            _marshaller.TryUnpack("127.0.0.1", out IPAddress address);

            Assert.AreEqual(IPAddress.Loopback, address);
        }
    }
}