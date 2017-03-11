using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetPrints.Graph;
using NetPrints.Core;
using NetPrints.Translator;

namespace NetPrints.Tests
{
    [TestClass]
    public class TranslatorTests
    {
        private Method method;

        [TestInitialize]
        public void CreateMethod()
        {
            List<Type> argumentTypes = new List<Type>()
            {
                typeof(string),
            };

            List<Type> returnTypes = new List<Type>()
            {
                typeof(int),
            };

            /*
             Func ->  Get Length -> Return
             Str ----> Str Int ---> Int
             */

            // Create nodes
            MethodEntryNode entryNode = new MethodEntryNode(argumentTypes);
            VariableGetterNode getLengthNode = new VariableGetterNode("Length", typeof(int));
            ReturnNode returnNode = new ReturnNode(returnTypes.ToArray());

            // Connect node execs
            entryNode.OutputExecPins[0].OutgoingPin = returnNode.InputExecPins[0];
            returnNode.InputExecPins[0].IncomingPins.Add(entryNode.OutputExecPins[0].OutgoingPin);

            // Connect node data
            entryNode.OutputDataPins[0].OutgoingPins.Add(getLengthNode.InputDataPins[0]);
            getLengthNode.InputDataPins[0].IncomingPin = entryNode.OutputDataPins[0];

            getLengthNode.OutputDataPins[0].OutgoingPins.Add(returnNode.InputDataPins[0]);
            returnNode.InputDataPins[0].IncomingPin = getLengthNode.OutputDataPins[0];

            // Create method
            method = new Method("TestMethod", argumentTypes.ToArray(), returnTypes.ToArray(), entryNode);
        }

        [TestMethod]
        public void TestMethodTranslator()
        {
            MethodTranslator methodTranslator = new MethodTranslator();
            string translated = methodTranslator.Translate(method);
        }
    }
}
