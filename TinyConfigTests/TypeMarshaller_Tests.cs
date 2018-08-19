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

namespace TinyConfig.Tests
{
    [TestFixture()]
    public class TypeMarshaller_Tests
    {
        [Test()]
        public void TryPack_DoubleArray()
        {
            var m = new DoubleMarshaller();
            m.TryPack(new double[] { -1, 2, 3.4, 4.59, 0 },  out ConfigValue result);

            var actual = result.Value;
            var expected = "-1 2 3.4 4.59 0";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void TryPackUnpack_DoubleArray()
        {
            var m = new DoubleMarshaller();
            var data1 = new double[] { };
            var data2 = new double[] { 1.1 };
            var data3 = new double[] { 1.1, 2.2, 3.3 };

            Assert.True(m.TryPack(data1, out ConfigValue result1));
            Assert.True(m.TryPack(data2, out ConfigValue result2));
            Assert.True(m.TryPack(data3, out ConfigValue result3));

            Assert.True(m.TryUnpack(result1, typeof(double), out object actualData1));
            Assert.True(m.TryUnpack(result2, typeof(double), out object actualData2));
            Assert.True(m.TryUnpack(result3, typeof(double), out object actualData3));

            Assert.AreEqual(data1, actualData1);
            Assert.AreEqual(data2, actualData2);
            Assert.AreEqual(data3, actualData3);
        }

        [Test()]
        public void TryPack_DoubleNaN()
        {
            var m = new DoubleMarshaller();
            Assert.True(m.TryPack(double.NaN, out ConfigValue result));

            var actual = result.Value;
            var expected = "NaN";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void TryPack_DoubleInifinity()
        {
            var m = new DoubleMarshaller();
            Assert.True(m.TryPack(double.NegativeInfinity, out ConfigValue result));

            var actual = result.Value;
            var expected = "-Infinity";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void TryUnpack_DoubleInifinity()
        {
            var m = new DoubleMarshaller();
            Assert.True(m.TryUnpack("-Infinity", out double actual));

            Assert.AreEqual(double.NegativeInfinity, actual);
        }

        [Test()]
        public void TryPackUnpack_DateTime()
        {
            var m = new DateTimeMarshaller();
            var date1 = DateTime.Now;
            var date2 = new DateTime(99999, DateTimeKind.Local);
            var date3 = new DateTime(99999, DateTimeKind.Unspecified);
            var date4 = new DateTime(99999, DateTimeKind.Utc);

            Assert.True(m.TryPack(date1, out ConfigValue result1));
            Assert.True(m.TryPack(date2, out ConfigValue result2));
            Assert.True(m.TryPack(date3, out ConfigValue result3));
            Assert.True(m.TryPack(date4, out ConfigValue result4));

            Assert.True(m.TryUnpack(result1.Value, out DateTime actualDate1));
            Assert.True(m.TryUnpack(result2.Value, out DateTime actualDate2));
            Assert.True(m.TryUnpack(result3.Value, out DateTime actualDate3));
            Assert.True(m.TryUnpack(result4.Value, out DateTime actualDate4));

            Assert.AreEqual(date1, actualDate1);
            Assert.AreEqual(date2, actualDate2);
            Assert.AreEqual(date3, actualDate3);
            Assert.AreEqual(date4, actualDate4);
        }

        [Test()]
        public void TryPackUnpack_DateTimeOffset()
        {
            var m = new DateTimeOffsetMarshaller();
            var date1 = DateTimeOffset.Now;
            var date2 = DateTimeOffset.UtcNow;

            Assert.True(m.TryPack(date1, out ConfigValue result1));
            Assert.True(m.TryPack(date2, out ConfigValue result2));

            Assert.True(m.TryUnpack(result1.Value, out DateTimeOffset actualDate1));
            Assert.True(m.TryUnpack(result2.Value, out DateTimeOffset actualDate2));

            Assert.AreEqual(date1, actualDate1);
            Assert.AreEqual(date2, actualDate2);
        }

        [Test()]
        public void TryPackUnpack_TimeSpan()
        {
            var m = new TimeSpanMarshaller();
            var date1 = TimeSpan.Zero;
            var date2 = new TimeSpan(9999999);
            var date3 = new TimeSpan(-9999999);
            var date4 = TimeSpan.MinValue;

            Assert.True(m.TryPack(date1, out ConfigValue result1));
            Assert.True(m.TryPack(date2, out ConfigValue result2));
            Assert.True(m.TryPack(date3, out ConfigValue result3));
            Assert.True(m.TryPack(date4, out ConfigValue result4));

            Assert.True(m.TryUnpack(result1.Value, out TimeSpan actualDate1));
            Assert.True(m.TryUnpack(result2.Value, out TimeSpan actualDate2));
            Assert.True(m.TryUnpack(result3.Value, out TimeSpan actualDate3));
            Assert.True(m.TryUnpack(result4.Value, out TimeSpan actualDate4));

            Assert.AreEqual(date1, actualDate1);
            Assert.AreEqual(date2, actualDate2);
            Assert.AreEqual(date3, actualDate3);
            Assert.AreEqual(date4, actualDate4);
        }

        [Test()]
        public void TryPackUnpack_List()
        {
            var m = new ListMarshaller();
            var data1 = new List<int>() { };
            var data2 = new List<int>() { 1 };
            var data3 = new List<int>() { 1, 2, 3 };

            Assert.True(m.TryPack(data1, out ConfigValue result1));
            Assert.True(m.TryPack(data2, out ConfigValue result2));
            Assert.True(m.TryPack(data3, out ConfigValue result3));

            Assert.True(m.TryUnpack(result1, typeof(List<int>), out object actualData1));
            Assert.True(m.TryUnpack(result2, typeof(List<int>), out object actualData2));
            Assert.True(m.TryUnpack(result3, typeof(List<int>), out object actualData3));

            Assert.AreEqual(data1, actualData1);
            Assert.AreEqual(data2, actualData2);
            Assert.AreEqual(data3, actualData3);
        }

        //[Test()]
        //public void TryPackUnpack_UnknownList()
        //{
        //    var m = new ListMarshaller();
        //    var data1 = new List<n>() { };
        //    var data2 = new List<int>() { 1 };
        //    var data3 = new List<int>() { 1, 2, 3 };

        //    Assert.True(m.TryPack(data1, out ConfigValue result1));
        //    Assert.True(m.TryPack(data2, out ConfigValue result2));
        //    Assert.True(m.TryPack(data3, out ConfigValue result3));

        //    Assert.True(m.TryUnpack(result1, typeof(List<int>), out object actualData1));
        //    Assert.True(m.TryUnpack(result2, typeof(List<int>), out object actualData2));
        //    Assert.True(m.TryUnpack(result3, typeof(List<int>), out object actualData3));

        //    Assert.AreEqual(data1, actualData1);
        //    Assert.AreEqual(data2, actualData2);
        //    Assert.AreEqual(data3, actualData3);
        //}

        [Test()]
        public void TryPack_StringArray()
        {
            var m = new StringMarshaller();
            Assert.Throws<ArrayPackingNotSupportedException>(() => m.TryPack(new string[] { "one", "two!" }, out ConfigValue result));
        }
    }
}