using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetPrints.Graph;
using NetPrints.Core;
using NetPrints.Extensions;
using NetPrints.Translator;

namespace NetPrints.Tests
{
    [TestClass]
    public class ClassTranslatorTests
    {
        private ClassTranslator classTranslator;
        private Method stringLengthMethod;
        private Method mainMethod;

        private Class cls;

        public void CreateStringLengthMethod()
        {
            List<Type> argumentTypes = new List<Type>()
            {
            };

            List<Type> returnTypes = new List<Type>()
            {
                typeof(int),
            };

            // Create method
            stringLengthMethod = new Method("StringLength");
            stringLengthMethod.Modifiers = MethodModifiers.Public;
            stringLengthMethod.ArgumentTypes.AddRange(argumentTypes);
            stringLengthMethod.ReturnTypes.AddRange(returnTypes);

            // Create nodes
            VariableGetterNode getStringNode = new VariableGetterNode(stringLengthMethod, "testVariable", typeof(string));
            VariableGetterNode getLengthNode = new VariableGetterNode(stringLengthMethod, "Length", typeof(int));

            // Connect node execs
            GraphUtil.ConnectExecPins(stringLengthMethod.EntryNode.InitialExecutionPin, stringLengthMethod.ReturnNode.ReturnPin);

            // Connect node data
            GraphUtil.ConnectDataPins(getStringNode.ValuePin, getLengthNode.TargetPin);
            GraphUtil.ConnectDataPins(getLengthNode.ValuePin, stringLengthMethod.ReturnNode.InputDataPins[0]);
        }

        public void CreateMainMethod()
        {
            List<Type> argumentTypes = new List<Type>()
            {
            };

            List<Type> returnTypes = new List<Type>()
            {
            };

            mainMethod = new Method("Main");
            mainMethod.Modifiers = MethodModifiers.Static;
            mainMethod.ArgumentTypes.AddRange(argumentTypes);
            mainMethod.ReturnTypes.AddRange(returnTypes);

            // Create nodes
            LiteralNode stringLiteralNode = new LiteralNode(mainMethod, typeof(string), "Hello World");
            VariableSetterNode setStringNode = new VariableSetterNode(mainMethod, "testVariable", typeof(string));
            CallMethodNode getStringLengthNode = new CallMethodNode(mainMethod, "StringLength", new List<Type>(), new List<Type>() { typeof(int) });
            CallStaticFunctionNode writeConsoleNode = new CallStaticFunctionNode(mainMethod, "Console", "WriteLine", new List<Type>() { typeof(string) }, new List<Type>());

            // Connect node execs
            GraphUtil.ConnectExecPins(mainMethod.EntryNode.InitialExecutionPin, setStringNode.InputExecPins[0]);
            GraphUtil.ConnectExecPins(setStringNode.OutputExecPins[0], getStringLengthNode.InputExecPins[0]);
            GraphUtil.ConnectExecPins(getStringLengthNode.OutputExecPins[0], writeConsoleNode.InputExecPins[0]);
            GraphUtil.ConnectExecPins(writeConsoleNode.OutputExecPins[0], mainMethod.ReturnNode.InputExecPins[0]);

            // Connect node data
            GraphUtil.ConnectDataPins(stringLiteralNode.ValuePin, setStringNode.NewValuePin);
            GraphUtil.ConnectDataPins(getStringLengthNode.OutputDataPins[0], writeConsoleNode.ArgumentPins[0]);
        }

        [TestInitialize]
        public void Setup()
        {
            CreateStringLengthMethod();
            CreateMainMethod();

            classTranslator = new ClassTranslator();

            cls = new Class();
            cls.Name = "TestClass";
            cls.Namespace = "TestNamespace";
            cls.SuperType = typeof(object);
            cls.Attributes.Add(new Variable("testVariable", typeof(string)));
            cls.Methods.Add(stringLengthMethod);
            cls.Methods.Add(mainMethod);
        }

        [TestMethod]
        public void TestClassTranslation()
        {
            string translated = classTranslator.TranslateClass(cls);
        }
    }
}
