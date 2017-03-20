using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Translator;
using System;
using System.Collections.Generic;

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
            List<Type> argumentTypes = new List<Type>()
            {
                typeof(string),
            };

            List<Type> returnTypes = new List<Type>()
            {
                typeof(int),
            };

            // Create method
            stringLengthMethod = new Method("StringLength")
            {
                Modifiers = MethodModifiers.Public
            };
            stringLengthMethod.ArgumentTypes.AddRange(argumentTypes);
            stringLengthMethod.ReturnTypes.AddRange(returnTypes);

            // Create nodes
            VariableGetterNode getLengthNode = new VariableGetterNode(stringLengthMethod, "Length", typeof(int));

            // Connect node execs
            GraphUtil.ConnectExecPins(stringLengthMethod.EntryNode.InitialExecutionPin, stringLengthMethod.ReturnNode.ReturnPin);

            // Connect node data
            GraphUtil.ConnectDataPins(stringLengthMethod.EntryNode.OutputDataPins[0], getLengthNode.InputDataPins[0]);
            GraphUtil.ConnectDataPins(getLengthNode.OutputDataPins[0], stringLengthMethod.ReturnNode.InputDataPins[0]);
        }
        
        public void CreateIfElseMethod()
        {
            List<Type> argumentTypes = new List<Type>()
            {
                typeof(int),
                typeof(bool),
            };

            List<Type> returnTypes = new List<Type>()
            {
                typeof(int),
            };

            // Create method
            ifElseMethod = new Method("IfElse")
            {
                Modifiers = MethodModifiers.Public
            };
            ifElseMethod.ArgumentTypes.AddRange(argumentTypes);
            ifElseMethod.ReturnTypes.AddRange(returnTypes);

            // Create nodes
            IfElseNode ifElseNode = new IfElseNode(ifElseMethod);
            LiteralNode literalNode = new LiteralNode(ifElseMethod, typeof(int), 123);

            // Connect exec nodes
            GraphUtil.ConnectExecPins(ifElseMethod.EntryNode.InitialExecutionPin, ifElseNode.ExecutionPin);
            GraphUtil.ConnectExecPins(ifElseNode.TruePin, ifElseMethod.ReturnNode.ReturnPin);
            GraphUtil.ConnectExecPins(ifElseNode.FalsePin, ifElseMethod.ReturnNode.ReturnPin);

            // Connect node data
            GraphUtil.ConnectDataPins(ifElseMethod.EntryNode.OutputDataPins[1], ifElseNode.ConditionPin);
            GraphUtil.ConnectDataPins(ifElseMethod.EntryNode.OutputDataPins[0], ifElseMethod.ReturnNode.InputDataPins[0]);
            GraphUtil.ConnectDataPins(literalNode.ValuePin, ifElseMethod.ReturnNode.InputDataPins[0]);
        }

        public void CreateForLoopMethod()
        {
            List<Type> argumentTypes = new List<Type>()
            {
            };

            List<Type> returnTypes = new List<Type>()
            {
            };

            // Create method
            forLoopMethod = new Method("ForLoop")
            {
                Modifiers = MethodModifiers.Public
            };
            forLoopMethod.ArgumentTypes.AddRange(argumentTypes);
            forLoopMethod.ReturnTypes.AddRange(returnTypes);

            // Create nodes
            LiteralNode maxIndexLiteralNode = new LiteralNode(forLoopMethod, typeof(int), 10);
            ForLoopNode forLoopNode = new ForLoopNode(forLoopMethod);
            
            // Connect exec nodes
            GraphUtil.ConnectExecPins(forLoopMethod.EntryNode.InitialExecutionPin, forLoopNode.ExecutionPin);
            GraphUtil.ConnectExecPins(forLoopNode.CompletedPin, forLoopMethod.ReturnNode.ReturnPin);

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
