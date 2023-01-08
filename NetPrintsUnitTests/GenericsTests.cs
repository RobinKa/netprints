﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Translator;
using System.Collections.Generic;
using System.Linq;

namespace NetPrintsUnitTests
{
    [TestClass]
    public class GenericsTests
    {
        [TestMethod]
        public void TestGenerics()
        {
            // Create the open class<T> which contains a List<T>

            GenericType genericClassArg = new GenericType("T");

            ClassGraph openClass = new ClassGraph();
            openClass.Name = "OpenClass";
            openClass.Namespace = "Namespace";
            openClass.DeclaredGenericArguments.Add(genericClassArg);

            TypeSpecifier listType = TypeSpecifier.FromType(typeof(List<>));

            Assert.AreEqual(listType.GenericArguments.Count, 1);

            listType.GenericArguments[0] = genericClassArg;

            MethodGraph openMethod = new MethodGraph("OpenMethod");

            // Add open list parameter
            TypeNode listTypeNode = new TypeNode(openMethod, listType);

            openMethod.MainReturnNode.AddReturnType();
            GraphUtil.ConnectPins(listTypeNode.OutputTypePins[0], openMethod.MainReturnNode.InputTypePins[0]);

            DefaultNode defaultNode = new DefaultNode(openMethod);
            GraphUtil.ConnectPins(listTypeNode.OutputTypePins[0], defaultNode.TypePin);
            GraphUtil.ConnectPins(defaultNode.DefaultValuePin, openMethod.MainReturnNode.InputDataPins[0]);

            GraphUtil.ConnectPins(openMethod.EntryNode.InitialExecutionPin, openMethod.ReturnNodes.First().ReturnPin);

            openClass.Methods.Add(openMethod);

            // Create the closed class which contains a List<string>

            ClassGraph closedClass = new ClassGraph();
            closedClass.Name = "ClosedClass";
            closedClass.Namespace = "Namespace";

            TypeSpecifier closedListType = TypeSpecifier.FromType<string>();

            MethodGraph closedMethod = new MethodGraph("ClosedMethod");

            // Add closed list parameter
            TypeNode closedListTypeNode = new TypeNode(closedMethod, closedListType);
            closedMethod.MainReturnNode.AddReturnType();
            GraphUtil.ConnectPins(closedListTypeNode.OutputTypePins[0], closedMethod.MainReturnNode.InputTypePins[0]);

            DefaultNode closedDefaultNode = new DefaultNode(closedMethod);
            GraphUtil.ConnectPins(closedListTypeNode.OutputTypePins[0], closedDefaultNode.TypePin);
            GraphUtil.ConnectPins(closedDefaultNode.DefaultValuePin, closedMethod.MainReturnNode.InputDataPins[0]);

            GraphUtil.ConnectPins(closedMethod.EntryNode.InitialExecutionPin, closedMethod.ReturnNodes.First().ReturnPin);

            closedClass.Methods.Add(closedMethod);

            // Translate the classes

            ClassTranslator translator = new ClassTranslator();
            
            string openClassTranslated = translator.TranslateClass(openClass);

            string closedClassTranslated = translator.TranslateClass(closedClass);
        }

        [TestMethod]
        public void TestTypeGraph()
        {
            MethodGraph method = new MethodGraph("Method");

            var unboundListType = new TypeSpecifier("System.Collections.Generic.List", genericArguments: new BaseType[] { new GenericType("T") });

            var literalNode = new LiteralNode(method, unboundListType);
            Assert.AreEqual(literalNode.InputTypePins.Count, 1);

            var typeNode = new TypeNode(method, TypeSpecifier.FromType<int>());
            Assert.AreEqual(literalNode.InputTypePins.Count, 1);

            GraphUtil.ConnectPins(typeNode.OutputTypePins[0], literalNode.InputTypePins[0]);
            Assert.AreEqual(literalNode.InputTypePins[0].InferredType.Value, new TypeSpecifier("System.Int32"));
            Assert.AreEqual(literalNode.OutputDataPins[0].PinType.Value, new TypeSpecifier("System.Collections.Generic.List", genericArguments: new BaseType[] { new TypeSpecifier("System.Int32") }));
        }
    }
}
