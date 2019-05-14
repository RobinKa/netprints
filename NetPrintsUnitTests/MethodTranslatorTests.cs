using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Translator;
using System.Collections.Generic;
using System.Linq;

namespace NetPrints.Tests
{
    [TestClass]
    public class MethodTranslatorTests
    {
        private MethodGraph stringLengthMethod;
        private MethodGraph ifElseMethod;
        private MethodGraph forLoopMethod;

        private ExecutionGraphTranslator methodTranslator;

        [TestInitialize]
        public void Setup()
        {
            methodTranslator = new ExecutionGraphTranslator();
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
            stringLengthMethod = new MethodGraph("StringLength")
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
                ((MethodEntryNode)stringLengthMethod.EntryNode).AddArgument();
                GraphUtil.ConnectPins(argTypeNodes[i].OutputTypePins[0], stringLengthMethod.EntryNode.InputTypePins[i]);
            }

            // Add return types
            List<TypeNode> returnTypeNodes = new List<TypeNode>()
            {
                new TypeNode(stringLengthMethod, TypeSpecifier.FromType<int>()),
            };

            for (int i = 0; i < returnTypeNodes.Count; i++)
            {
                stringLengthMethod.MainReturnNode.AddReturnType();
                GraphUtil.ConnectPins(returnTypeNodes[i].OutputTypePins[0], stringLengthMethod.MainReturnNode.InputTypePins[i]);
            }

            // Create nodes
            var getLengthNode = new VariableGetterNode(stringLengthMethod, new VariableSpecifier("Length", TypeSpecifier.FromType<int>(),
                MemberVisibility.Public,MemberVisibility.Public, TypeSpecifier.FromType<string>(), VariableModifiers.None));

            // Connect node execs
            GraphUtil.ConnectPins(stringLengthMethod.EntryNode.InitialExecutionPin, stringLengthMethod.ReturnNodes.First().ReturnPin);

            // Connect node data
            GraphUtil.ConnectPins(stringLengthMethod.EntryNode.OutputDataPins[0], getLengthNode.InputDataPins[0]);
            GraphUtil.ConnectPins(getLengthNode.OutputDataPins[0], stringLengthMethod.ReturnNodes.First().InputDataPins[0]);
        }
        
        public void CreateIfElseMethod()
        {
            // Create method
            ifElseMethod = new MethodGraph("IfElse")
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
                ((MethodEntryNode)ifElseMethod.EntryNode).AddArgument();
                GraphUtil.ConnectPins(argTypeNodes[i].OutputTypePins[0], ifElseMethod.EntryNode.InputTypePins[i]);
            }

            // Add return types
            List<TypeNode> returnTypeNodes = new List<TypeNode>()
            {
                new TypeNode(ifElseMethod, TypeSpecifier.FromType<int>()),
            };

            for (int i = 0; i < returnTypeNodes.Count; i++)
            {
                ifElseMethod.MainReturnNode.AddReturnType();
                GraphUtil.ConnectPins(returnTypeNodes[i].OutputTypePins[0], ifElseMethod.MainReturnNode.InputTypePins[i]);
            }

            // Create nodes
            IfElseNode ifElseNode = new IfElseNode(ifElseMethod);
            LiteralNode literalNode = LiteralNode.WithValue(ifElseMethod, 123);

            // Connect exec nodes
            GraphUtil.ConnectPins(ifElseMethod.EntryNode.InitialExecutionPin, ifElseNode.ExecutionPin);
            GraphUtil.ConnectPins(ifElseNode.TruePin, ifElseMethod.ReturnNodes.First().ReturnPin);
            GraphUtil.ConnectPins(ifElseNode.FalsePin, ifElseMethod.ReturnNodes.First().ReturnPin);

            // Connect node data
            GraphUtil.ConnectPins(ifElseMethod.EntryNode.OutputDataPins[1], ifElseNode.ConditionPin);
            GraphUtil.ConnectPins(ifElseMethod.EntryNode.OutputDataPins[0], ifElseMethod.ReturnNodes.First().InputDataPins[0]);
            GraphUtil.ConnectPins(literalNode.ValuePin, ifElseMethod.ReturnNodes.First().InputDataPins[0]);
        }

        public void CreateForLoopMethod()
        {
            // Create method
            forLoopMethod = new MethodGraph("ForLoop")
            {
                Visibility = MemberVisibility.Public
            };

            // Create nodes
            LiteralNode maxIndexLiteralNode = LiteralNode.WithValue(forLoopMethod, 10);
            ForLoopNode forLoopNode = new ForLoopNode(forLoopMethod);
            
            // Connect exec nodes
            GraphUtil.ConnectPins(forLoopMethod.EntryNode.InitialExecutionPin, forLoopNode.ExecutionPin);
            GraphUtil.ConnectPins(forLoopNode.CompletedPin, forLoopMethod.ReturnNodes.First().ReturnPin);

            // Connect node data
            GraphUtil.ConnectPins(maxIndexLiteralNode.ValuePin, forLoopNode.MaxIndexPin);
        }

        [TestMethod]
        public void TestStringLengthTranslation()
        {
            string translated = methodTranslator.Translate(stringLengthMethod, true);
        }

        [TestMethod]
        public void TestIfElseTranslation()
        {
            string translated = methodTranslator.Translate(ifElseMethod, true);
        }

        [TestMethod]
        public void TestForLoopTranslation()
        {
            string translated = methodTranslator.Translate(forLoopMethod, true);
        }
    }
}
