﻿using NUnit.Framework;
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
    public class ObjectMarshallers_IntegrationalTests
    {
        [Serializable]
        struct FlatStructWithPrivateFields
        {
            double X1;
            internal double X2;
            public double X3;
            public readonly double X4;

            public FlatStructWithPrivateFields(double x1, double x2, double x3, double x4)
            {
                X1 = x1;
                X2 = x2;
                X3 = x3;
                X4 = x4;
            }
        }

        [Serializable]
        struct NotFlatStruct
        {
            public V2 V1;

            public NotFlatStruct(V2 v1)
            {
                V1 = v1;
            }
        }

        //class TestObject
        //{
        //    string Text = "Hello";
        //}
        //struct NotFlatStructWithObject
        //{
        //    public TestObject Obj;

        //    public NotFlatStructWithObject(TestObject obj)
        //    {
        //        Obj = obj;
        //    }
        //}

        [Serializable]
        struct MixedFlatnessStruct
        {
            public double X1;
            public V2 V1;

            public MixedFlatnessStruct(double x1, V2 v1)
            {
                X1 = x1;
                V1 = v1;
            }
        }

        [Serializable]
        struct ThreeLevelNotFlatStruct
        {
            public MixedFlatnessStruct Struct;
            public double SomeValue;

            public ThreeLevelNotFlatStruct(MixedFlatnessStruct @struct, double someValue)
            {
                Struct = @struct;
                SomeValue = someValue;
            }
        }

        [Test()]
        public void TryPack_FlatStructure()
        {
            var config = Configurable.CreateConfig("TryPack_FlatStructure", "ObjectMarshaller_IntegrationalTests").Clear();
            var proxy = new ConfigAccessorProxy(config, "Object1");

            var m = new FlatStructObjectMarshaller();
            Assert.True(m.TryPack(proxy, new V2(1, 2)));

            var actual = config.ToString();
            var expected = @"[Object1]
X=1
Y=2
";

            Assert.AreEqual(expected, actual);
        }


        [Test()]
        public void TryPack_FlatStructureWithPrivateFields()
        {
            var config = (ConfigAccessor)Configurable.CreateConfig("TryPack_FlatStructureWithPrivateFields", "ObjectMarshaller_IntegrationalTests").Clear();
            var proxy = new ConfigAccessorProxy(config, "Object1");

            var m = new FlatStructObjectMarshaller();
            Assert.True(m.TryPack(proxy, new FlatStructWithPrivateFields(1, 2, 3, 4)));

            var actual = config.ToString();
            var expected = @"[Object1]
X1=1
X2=2
X3=3
X4=4
";

            Assert.AreEqual(expected, actual);
        }


        [Test()]
        public void TryPack_NotFlatStructure()
        {
            var config = (ConfigAccessor)Configurable.CreateConfig("TryPack_NotFlatStructure", "ObjectMarshaller_IntegrationalTests").Clear();
            var proxy = new ConfigAccessorProxy(config, "Object1");

            var m = new FlatStructObjectMarshaller();
            Assert.True(m.TryPack(proxy, new NotFlatStruct(new V2(1, 2))));

            var actual = config.ToString();
            var expected = @"[Object1]
[Object1.V1]
X=1
Y=2
";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void TryPack_MixedFlatnessStructure()
        {
            var config = (ConfigAccessor)Configurable.CreateConfig("TryPack_MixedFlatnessStructure", "ObjectMarshaller_IntegrationalTests").Clear();
            var proxy = new ConfigAccessorProxy(config, "Object1");

            var m = new FlatStructObjectMarshaller();
            Assert.True(m.TryPack(proxy, new MixedFlatnessStruct(1, new V2(2, 3))));

            var actual = config.ToString();
            var expected = @"[Object1]
X1=1
[Object1.V1]
X=2
Y=3
";

            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void TryPack_ThreeLevelNotFlatStruct()
        {
            var config = (ConfigAccessor)Configurable.CreateConfig("TryPack_ThreeLevelNotFlatStruct", "ObjectMarshaller_IntegrationalTests").Clear();
            var proxy = new ConfigAccessorProxy(config, "Object1");

            var m = new FlatStructObjectMarshaller();
            Assert.True(m.TryPack(proxy, new ThreeLevelNotFlatStruct(new MixedFlatnessStruct(1, new V2(2, 3)), 4)));

            var actual = config.ToString();
            var expected = @"[Object1]
SomeValue=4
[Object1.Struct]
X1=1
[Object1.Struct.V1]
X=2
Y=3
";

            Assert.AreEqual(expected, actual);
        }
    }
}