using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Translator;
using System;
using System.Collections.Generic;

namespace NetPrints.Tests
{
    [TestClass]
    public class DelegateTranslatorTests
    {
        private MethodTranslator methodTranslator;

        [TestInitialize]
        public void Setup()
        {
            methodTranslator = new MethodTranslator();
        }

        [TestMethod]
        public void TestDelegate()
        {
            List<TypeSpecifier> argumentTypes = new List<TypeSpecifier>()
            {
            };

            List<TypeSpecifier> returnTypes = new List<TypeSpecifier>()
            {
                TypeSpecifier.FromType<Func<int, string, float>>(),
            };

            // Create method
            Method delegateMethod = new Method("DelegateMethod")
            {
                Modifiers = MethodModifiers.Public
            };

            delegateMethod.ArgumentTypes.AddRange(argumentTypes);
            delegateMethod.ReturnTypes.AddRange(returnTypes);

            MethodSpecifier delegateMethodSpecifier = new MethodSpecifier("TestMethod",
                new Named<BaseType>[] { new Named<BaseType>("arg1", TypeSpecifier.FromType<int>()), new Named<BaseType>("arg2", TypeSpecifier.FromType<string>()) },
                new BaseType[] { TypeSpecifier.FromType<float>() },
                MethodModifiers.Static,
                TypeSpecifier.FromType<double>(),
                Array.Empty<BaseType>());

            // Create nodes
            MakeDelegateNode makeDelegateNode = new MakeDelegateNode(delegateMethod, delegateMethodSpecifier);

            // Connect node execs
            GraphUtil.ConnectExecPins(delegateMethod.EntryNode.InitialExecutionPin, delegateMethod.ReturnNode.ReturnPin);

            // Connect node data
            GraphUtil.ConnectDataPins(makeDelegateNode.OutputDataPins[0], delegateMethod.ReturnNode.InputDataPins[0]);

            string translated = methodTranslator.Translate(delegateMethod);
        }
    }
}
