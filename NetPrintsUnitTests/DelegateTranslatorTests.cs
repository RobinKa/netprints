using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Translator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetPrints.Tests
{
    [TestClass]
    public class DelegateTranslatorTests
    {
        private ExecutionGraphTranslator methodTranslator;

        [TestInitialize]
        public void Setup()
        {
            methodTranslator = new ExecutionGraphTranslator();
        }

        [TestMethod]
        public void TestDelegate()
        {
            // Create method
            MethodGraph delegateMethod = new MethodGraph("DelegateMethod")
            {
                Visibility = MemberVisibility.Public
            };

            List<TypeNode> returnTypeNodes = new List<TypeNode>()
            {
                new TypeNode(delegateMethod, TypeSpecifier.FromType<Func<int, string, float>>()),
            };

            for (int i = 0; i < returnTypeNodes.Count; i++)
            {
                delegateMethod.MainReturnNode.AddReturnType();
                GraphUtil.ConnectTypePins(returnTypeNodes[i].OutputTypePins[0], delegateMethod.MainReturnNode.InputTypePins[i]);
            }

            MethodSpecifier delegateMethodSpecifier = new MethodSpecifier("TestMethod",
                new MethodParameter[]
                {
                    new MethodParameter("arg1", TypeSpecifier.FromType<int>(), MethodParameterPassType.Default),
                    new MethodParameter("arg2", TypeSpecifier.FromType<string>(), MethodParameterPassType.Default)
                },
                new BaseType[] { TypeSpecifier.FromType<float>() },
                MethodModifiers.Static, MemberVisibility.Public,
                TypeSpecifier.FromType<double>(), Array.Empty<BaseType>());

            // Create nodes
            MakeDelegateNode makeDelegateNode = new MakeDelegateNode(delegateMethod, delegateMethodSpecifier);

            // Connect node execs
            GraphUtil.ConnectExecPins(delegateMethod.EntryNode.InitialExecutionPin, delegateMethod.ReturnNodes.First().ReturnPin);

            // Connect node data
            GraphUtil.ConnectDataPins(makeDelegateNode.OutputDataPins[0], delegateMethod.ReturnNodes.First().InputDataPins[0]);

            string translated = methodTranslator.Translate(delegateMethod, true, false);
        }
    }
}
