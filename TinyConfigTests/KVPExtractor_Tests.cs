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
                new ConfigKVP ("KEY1", new ConfigValue("Value", false)),
                new ConfigKVP ("fff", new ConfigValue("2", false)),
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
                new ConfigKVP("KEY1", new ConfigValue("Value 'line1'" + Global.NL + "line2" + Global.NL + "l'i'ne3", true)),
                new ConfigKVP ("KEY2", new ConfigValue("Hello 'friend'!", true)),
                new ConfigKVP ("KEY3", new ConfigValue("'Hel''lo ''", true)),
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
                new ConfigKVP ("KEY1", new ConfigValue(" Value", false)),
                new ConfigKVP("KEY2", new ConfigValue("Hello 'friend'!", true)),
                new ConfigKVP("KEY3", new ConfigValue(" eeHel''''lo", false)),
            };

            Assert.AreEqual(expected, actual);
        }
    }
}