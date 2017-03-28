using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetPrints.Core;

namespace NetPrintsUnitTests
{
    [TestClass]
    public class TypeTests
    {
        [TestMethod]
        public void TestEquality()
        {
            TypeSpecifier typeA = new TypeSpecifier("TypeA");
            TypeSpecifier typeB = new TypeSpecifier("TypeB");
            TypeSpecifier sameAsTypeA = new TypeSpecifier("TypeA");

            Assert.AreNotEqual(typeA, typeB);
            Assert.AreEqual(typeA, sameAsTypeA);
            Assert.AreNotEqual(sameAsTypeA, typeB);
        }

        [TestMethod]
        public void TestGenericEquality()
        {
            TypeSpecifier typeA = new TypeSpecifier("TypeA", false, false, new BaseType[] { });
            GenericType genType1 = new GenericType("T1");
            GenericType genType2 = new GenericType("T2");

            Assert.AreEqual(typeA, genType1);
            Assert.AreEqual(typeA, genType2);
            Assert.AreNotEqual(genType1, genType2);
        }

        [TestMethod]
        public void TestTypeConversionEquality()
        {
            TypeSpecifier typeInt = typeof(int);

            Assert.AreEqual(typeInt, typeof(int));
            Assert.AreEqual(typeof(int), typeInt);

            Assert.AreNotEqual(typeof(string), typeInt);
            Assert.AreNotEqual(typeInt, typeof(string));
        }

        [TestMethod]
        public void TestGenericTypeConversionEquality()
        {
            TypeSpecifier typeInt = typeof(System.Collections.Generic.List<int>);

            Assert.AreEqual(typeInt, typeof(System.Collections.Generic.List<int>));
            Assert.AreEqual(typeof(System.Collections.Generic.List<int>), typeInt);

            Assert.AreNotEqual(typeInt, typeof(System.Collections.Generic.List<string>));
            Assert.AreNotEqual(typeof(System.Collections.Generic.List<string>), typeInt);

            Assert.AreNotEqual(typeInt, typeof(System.Collections.Generic.Stack<string>));
            Assert.AreNotEqual(typeof(System.Collections.Generic.Stack<string>), typeInt);
        }
    }
}
