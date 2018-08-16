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
    public class ObjectMarshaller_Tests
    {
        class dd : IConfigAccessorProxy
        {
            public bool HasValueMarshaller(Type type)
            {
                throw new NotImplementedException();
            }

            public ConfigProxy<T> ReadValue<T>([CallerMemberName] string key = "")
            {
                throw new NotImplementedException();
            }

            public ConfigProxy<object> ReadValue(Type supposedType, [CallerMemberName] string key = "")
            {
                throw new NotImplementedException();
            }

            public ConfigProxy<T> ReadValueFrom<T>(string subsection, [CallerMemberName] string key = "")
            {
                throw new NotImplementedException();
            }

            public bool WriteValue<T>(T value, [CallerMemberName] string key = "")
            {
                throw new NotImplementedException();
            }

            public bool WriteValueFrom<T>(T value, string subsection, [CallerMemberName] string key = "")
            {
                throw new NotImplementedException();
            }
        }

        [Test()]
        public void TryPack_FlatStructure()
        {
            var m = new FlatStructObjectMarshaller();
            m.TryPack( new V2(1, 2));

            var actual = result.Value;
            var expected = "-1 2 3.4 4.59 0";

            Assert.AreEqual(expected, actual);
        }

    }
}