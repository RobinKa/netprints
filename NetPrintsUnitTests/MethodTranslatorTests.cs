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
    public class MethodTranslatorTests
    {
        private Method stringLengthMethod;
        private Method ifElseMethod;
        private Method forLoopMethod;

        private MethodTranslator methodTranslator;

        [TestInitialize]
        public void Setup()
        {
            methodTranslator = new MethodTranslator();
            CreateStringLengthMethod();
            CreateIfElseMethod();
            CreateForLoopMethod();
        }

        public void CreateStringLengthMethod()
        {
            List<TypeSpecifier> argumentTypes = new List<TypeSpecifier>()
            {
                TypeSpecifier.FromType<string>(),
            };

            // Create method
            stringLengthMethod = new Method("StringLength")
            {
                Visibility = MemberVisibility.Public
            };

            // Add arguments
            List<TypeNode> argTypeNodes = new List<TypeNode>()
            {
                new TypeNode(stringLengthMethod, TypeSpecifier.FromType<string>()),
            };

            for (int i = 0; i < argTypeNodes.Count; i++)
            {
                stringLengthMethod.EntryNode.AddArgument();
                GraphUtil.ConnectTypePins(argTypeNodes[i].OutputTypePins[0], stringLengthMethod.EntryNode.InputTypePins[i]);
            }

            // Add return types
            List<TypeNode> returnTypeNodes = new List<TypeNode>()
            {
                new TypeNode(stringLengthMethod, TypeSpecifier.FromType<int>()),
            };

            for (int i = 0; i < returnTypeNodes.Count; i++)
            {
                stringLengthMethod.MainReturnNode.AddReturnType();
                GraphUtil.ConnectTypePins(returnTypeNodes[i].OutputTypePins[0], stringLengthMethod.MainReturnNode.InputTypePins[i]);
            }

            // Create nodes
            VariableGetterNode getLengthNode = new VariableGetterNode(stringLengthMethod, TypeSpecifier.FromType<string>(), new Variable("Length", TypeSpecifier.FromType<int>()));

            // Connect node execs
            GraphUtil.ConnectExecPins(stringLengthMethod.EntryNode.InitialExecutionPin, stringLengthMethod.ReturnNodes.First().ReturnPin);

            // Connect node data
            GraphUtil.ConnectDataPins(stringLengthMethod.EntryNode.OutputDataPins[0], getLengthNode.InputDataPins[0]);
            GraphUtil.ConnectDataPins(getLengthNode.OutputDataPins[0], stringLengthMethod.ReturnNodes.First().InputDataPins[0]);
        }
        
        public void CreateIfElseMethod()
        {
            // Create method
            ifElseMethod = new Method("IfElse")
            {
                Visibility = MemberVisibility.Public
            };

            // Add arguments
            List<TypeNode> argTypeNodes = new List<TypeNode>()
            {
                new TypeNode(ifElseMethod, TypeSpecifier.FromType<int>()),
                new TypeNode(ifElseMethod, TypeSpecifier.FromType<bool>()),
            };

            for (int i = 0; i < argTypeNodes.Count; i++)
            {
                ifElseMethod.EntryNode.AddArgument();
                GraphUtil.ConnectTypePins(argTypeNodes[i].OutputTypePins[0], ifElseMethod.EntryNode.InputTypePins[i]);
            }

            // Add return types
            List<TypeNode> returnTypeNodes = new List<TypeNode>()
            {
                new TypeNode(ifElseMethod, TypeSpecifier.FromType<int>()),
            };

            for (int i = 0; i < returnTypeNodes.Count; i++)
            {
                ifElseMethod.MainReturnNode.AddReturnType();
                GraphUtil.ConnectTypePins(returnTypeNodes[i].OutputTypePins[0], ifElseMethod.MainReturnNode.InputTypePins[i]);
            }

            // Create nodes
            IfElseNode ifElseNode = new IfElseNode(ifElseMethod);
            LiteralNode literalNode = LiteralNode.WithValue(ifElseMethod, 123);

            // Connect exec nodes
            GraphUtil.ConnectExecPins(ifElseMethod.EntryNode.InitialExecutionPin, ifElseNode.ExecutionPin);
            GraphUtil.ConnectExecPins(ifElseNode.TruePin, ifElseMethod.ReturnNodes.First().ReturnPin);
            GraphUtil.ConnectExecPins(ifElseNode.FalsePin, ifElseMethod.ReturnNodes.First().ReturnPin);

            // Connect node data
            GraphUtil.ConnectDataPins(ifElseMethod.EntryNode.OutputDataPins[1], ifElseNode.ConditionPin);
            GraphUtil.ConnectDataPins(ifElseMethod.EntryNode.OutputDataPins[0], ifElseMethod.ReturnNodes.First().InputDataPins[0]);
            GraphUtil.ConnectDataPins(literalNode.ValuePin, ifElseMethod.ReturnNodes.First().InputDataPins[0]);
        }

        public void CreateForLoopMethod()
        {
            // Create method
            forLoopMethod = new Method("ForLoop")
            {
                Visibility = MemberVisibility.Public
            };

            // Create nodes
            LiteralNode maxIndexLiteralNode = LiteralNode.WithValue(forLoopMethod, 10);
            ForLoopNode forLoopNode = new ForLoopNode(forLoopMethod);
            
            // Connect exec nodes
            GraphUtil.ConnectExecPins(forLoopMethod.EntryNode.InitialExecutionPin, forLoopNode.ExecutionPin);
            GraphUtil.ConnectExecPins(forLoopNode.CompletedPin, forLoopMethod.ReturnNodes.First().ReturnPin);

            // Connect node data
            GraphUtil.ConnectDataPins(maxIndexLiteralNode.ValuePin, forLoopNode.MaxIndexPin);
        }

        [TestMethod]
        public void TestStringLengthTranslation()
        {
            string translated = methodTranslator.Translate(stringLengthMethod);
        }

        [TestMethod]
        public void TestIfElseTranslation()
        {
            string translated = methodTranslator.Translate(ifElseMethod);
        }

        [TestMethod]
        public void TestForLoopTranslation()
        {
            string translated = methodTranslator.Translate(forLoopMethod);
        }
    }
}
