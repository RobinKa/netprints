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
        private MethodGraph stringLengthMethod;
        private MethodGraph mainMethod;

        private ClassGraph cls;

        public void CreateStringLengthMethod()
        {
            // Create method
            stringLengthMethod = new MethodGraph("StringLength")
            {
                Class = cls,
                Modifiers = MethodModifiers.None
            };

            List<TypeNode> returnTypeNodes = new List<TypeNode>()
            {
                new TypeNode(stringLengthMethod, TypeSpecifier.FromType<int>()),
            };

            for (int i = 0; i < returnTypeNodes.Count; i++)
            {
                stringLengthMethod.MainReturnNode.AddReturnType();
                GraphUtil.ConnectTypePins(returnTypeNodes[i].OutputTypePins[0], stringLengthMethod.MainReturnNode.InputTypePins[i]);
            }

            TypeSpecifier stringType = TypeSpecifier.FromType<string>();
            TypeSpecifier intType = TypeSpecifier.FromType<int>();

            // Create nodes
            VariableGetterNode getStringNode = new VariableGetterNode(stringLengthMethod, new VariableSpecifier("testVariable", stringType, MemberVisibility.Public, MemberVisibility.Public, stringType, VariableModifiers.None));
            VariableGetterNode getLengthNode = new VariableGetterNode(stringLengthMethod, new VariableSpecifier("Length", intType, MemberVisibility.Public, MemberVisibility.Public, stringType, VariableModifiers.None));

            // Connect node execs
            GraphUtil.ConnectExecPins(stringLengthMethod.EntryNode.InitialExecutionPin, stringLengthMethod.ReturnNodes.First().ReturnPin);

            // Connect node data
            GraphUtil.ConnectDataPins(getStringNode.ValuePin, getLengthNode.TargetPin);
            GraphUtil.ConnectDataPins(getLengthNode.ValuePin, stringLengthMethod.ReturnNodes.First().InputDataPins[0]);
        }

        public void CreateMainMethod()
        {
            mainMethod = new MethodGraph("Main")
            {
                Class = cls,
                Modifiers = MethodModifiers.Static
            };

            MethodSpecifier stringLengthSpecifier = new MethodSpecifier("StringLength", new MethodParameter[0], new List<TypeSpecifier>() { TypeSpecifier.FromType<int>() },
                MethodModifiers.None, MemberVisibility.Public, TypeSpecifier.FromType<string>(), Array.Empty<BaseType>());
            //MethodSpecifier writeConsoleSpecifier = typeof(Console).GetMethods().Single(m => m.Name == "WriteLine" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
            TypeSpecifier stringType = TypeSpecifier.FromType<string>();
            MethodSpecifier writeConsoleSpecifier = new MethodSpecifier("WriteLine", new MethodParameter[] { new MethodParameter("argName", stringType, MethodParameterPassType.Default) }, new BaseType[0],
                MethodModifiers.None, MemberVisibility.Public, TypeSpecifier.FromType(typeof(Console)), new BaseType[0]);

            // Create nodes
            LiteralNode stringLiteralNode = LiteralNode.WithValue(mainMethod, "Hello World");
            VariableSetterNode setStringNode = new VariableSetterNode(mainMethod, new VariableSpecifier("testVariable", TypeSpecifier.FromType<string>(), MemberVisibility.Public, MemberVisibility.Public, cls.Type, VariableModifiers.None));
            CallMethodNode getStringLengthNode = new CallMethodNode(mainMethod, stringLengthSpecifier);
            CallMethodNode writeConsoleNode = new CallMethodNode(mainMethod, writeConsoleSpecifier);

            // Connect node execs
            GraphUtil.ConnectExecPins(mainMethod.EntryNode.InitialExecutionPin, setStringNode.InputExecPins[0]);
            GraphUtil.ConnectExecPins(setStringNode.OutputExecPins[0], getStringLengthNode.InputExecPins[0]);
            GraphUtil.ConnectExecPins(getStringLengthNode.OutputExecPins[0], writeConsoleNode.InputExecPins[0]);
            GraphUtil.ConnectExecPins(writeConsoleNode.OutputExecPins[0], mainMethod.ReturnNodes.First().InputExecPins[0]);

            // Connect node data
            GraphUtil.ConnectDataPins(stringLiteralNode.ValuePin, setStringNode.NewValuePin);
            GraphUtil.ConnectDataPins(getStringLengthNode.OutputDataPins[0], writeConsoleNode.ArgumentPins[0]);
        }

        [TestInitialize]
        public void Setup()
        {
            classTranslator = new ClassTranslator();

            cls = new ClassGraph()
            {
                Name = "TestClass",
                Namespace = "TestNamespace",
            };

            CreateStringLengthMethod();
            CreateMainMethod();

            cls.Variables.Add(new Variable(cls, "testVariable", TypeSpecifier.FromType<string>(), null, null, VariableModifiers.None));
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
