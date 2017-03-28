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
    public class ClassTranslatorTests
    {
        private ClassTranslator classTranslator;
        private Method stringLengthMethod;
        private Method mainMethod;

        private Class cls;

        public void CreateStringLengthMethod()
        {
            List<TypeSpecifier> returnTypes = new List<TypeSpecifier>()
            {
                typeof(int),
            };

            // Create method
            stringLengthMethod = new Method("StringLength")
            {
                Class = cls,
                Modifiers = MethodModifiers.Public
            };

            stringLengthMethod.ReturnTypes.AddRange(returnTypes);

            // Create nodes
            VariableGetterNode getStringNode = new VariableGetterNode(stringLengthMethod, cls.Type, "testVariable", typeof(string));
            VariableGetterNode getLengthNode = new VariableGetterNode(stringLengthMethod, cls.Type, "Length", typeof(int));

            // Connect node execs
            GraphUtil.ConnectExecPins(stringLengthMethod.EntryNode.InitialExecutionPin, stringLengthMethod.ReturnNode.ReturnPin);

            // Connect node data
            GraphUtil.ConnectDataPins(getStringNode.ValuePin, getLengthNode.TargetPin);
            GraphUtil.ConnectDataPins(getLengthNode.ValuePin, stringLengthMethod.ReturnNode.InputDataPins[0]);
        }

        public void CreateMainMethod()
        {
            mainMethod = new Method("Main")
            {
                Class = cls,
                Modifiers = MethodModifiers.Static
            };

            MethodSpecifier stringLengthSpecifier = new MethodSpecifier("StringLength", new List<TypeSpecifier>(), new List<TypeSpecifier>() { typeof(int) }, MethodModifiers.Public, typeof(string), Array.Empty<BaseType>());
            MethodSpecifier writeConsoleSpecifier = typeof(Console).GetMethods().Single(m => m.Name == "WriteLine" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));

            // Create nodes
            LiteralNode stringLiteralNode = new LiteralNode(mainMethod, typeof(string), "Hello World");
            VariableSetterNode setStringNode = new VariableSetterNode(mainMethod, cls.Type, "testVariable", typeof(string));
            CallMethodNode getStringLengthNode = new CallMethodNode(mainMethod, stringLengthSpecifier);
            CallMethodNode writeConsoleNode = new CallMethodNode(mainMethod, writeConsoleSpecifier);

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
            classTranslator = new ClassTranslator();

            cls = new Class()
            {
                Name = "TestClass",
                Namespace = "TestNamespace",
                SuperType = typeof(object)
            };

            CreateStringLengthMethod();
            CreateMainMethod();

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
