using System.Collections.Generic;
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
            TypeSpecifier typeInt = TypeSpecifier.FromType<int>();

            Assert.AreEqual(typeInt, TypeSpecifier.FromType(typeof(int)));
            Assert.AreEqual(TypeSpecifier.FromType(typeof(int)), typeInt);

            Assert.AreNotEqual(TypeSpecifier.FromType(typeof(string)), typeInt);
            Assert.AreNotEqual(typeInt, TypeSpecifier.FromType(typeof(string)));
        }

        [TestMethod]
        public void TestGenericTypeConversionEquality()
        {
            TypeSpecifier typeInt = TypeSpecifier.FromType<List<int>>();

            Assert.AreEqual(typeInt, TypeSpecifier.FromType<List<int>>());
            Assert.AreEqual(TypeSpecifier.FromType<List<int>>(), typeInt);

            Assert.AreNotEqual(typeInt, TypeSpecifier.FromType<List<string>>());
            Assert.AreNotEqual(TypeSpecifier.FromType<List<string>>(), typeInt);

            Assert.AreNotEqual(typeInt, TypeSpecifier.FromType<Stack<string>>());
            Assert.AreNotEqual(TypeSpecifier.FromType<Stack<string>>(), typeInt);
        }
    }
}
