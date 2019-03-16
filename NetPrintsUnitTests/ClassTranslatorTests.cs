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
                TypeSpecifier.FromType<int>(),
            };

            // Create method
            stringLengthMethod = new Method("StringLength")
            {
                Class = cls,
                Modifiers = MethodModifiers.Public
            };

            stringLengthMethod.ReturnTypes.AddRange(returnTypes);

            // Create nodes
            VariableGetterNode getStringNode = new VariableGetterNode(stringLengthMethod, cls.Type, new Variable("testVariable", TypeSpecifier.FromType<string>()));
            VariableGetterNode getLengthNode = new VariableGetterNode(stringLengthMethod, cls.Type, new Variable("Length", TypeSpecifier.FromType<int>()));

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

            MethodSpecifier stringLengthSpecifier = new MethodSpecifier("StringLength", new Named<BaseType>[0], new List<TypeSpecifier>() { TypeSpecifier.FromType<int>() }, MethodModifiers.Public, TypeSpecifier.FromType<string>(), Array.Empty<BaseType>());
            //MethodSpecifier writeConsoleSpecifier = typeof(Console).GetMethods().Single(m => m.Name == "WriteLine" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
            TypeSpecifier stringType = TypeSpecifier.FromType<string>();
            MethodSpecifier writeConsoleSpecifier = new MethodSpecifier("WriteLine", new Named<BaseType>[] { new Named<BaseType>("argName", stringType) }, new BaseType[0], MethodModifiers.Public, TypeSpecifier.FromType(typeof(Console)), new BaseType[0]);

            // Create nodes
            LiteralNode stringLiteralNode = new LiteralNode(mainMethod, TypeSpecifier.FromType<string>(), "Hello World");
            VariableSetterNode setStringNode = new VariableSetterNode(mainMethod, cls.Type, new Variable("testVariable", TypeSpecifier.FromType<string>()));
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
                SuperType = TypeSpecifier.FromType<object>()
            };

            CreateStringLengthMethod();
            CreateMainMethod();

            cls.Attributes.Add(new Variable("testVariable", TypeSpecifier.FromType<string>()));
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
