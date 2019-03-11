using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsUnitTests
{
    [TestClass]
    public class GenericsTests
    {
        [TestMethod]
        public void TestGenerics()
        {
            // Create the open class<T> which contains a List<T>

            GenericType genericClassArg = new GenericType("T");

            Class openClass = new Class();
            openClass.Name = "OpenClass";
            openClass.Namespace = "Namespace";
            openClass.DeclaredGenericArguments.Add(genericClassArg);

            TypeSpecifier listType = TypeSpecifier.FromType(typeof(List<>));

            Assert.AreEqual(listType.GenericArguments.Count, 1);

            listType.GenericArguments[0] = genericClassArg;

            Method openMethod = new Method("OpenMethod");
            openMethod.ArgumentTypes.Add(listType);
            GraphUtil.ConnectExecPins(openMethod.EntryNode.InitialExecutionPin, openMethod.ReturnNode.ReturnPin);

            openClass.Methods.Add(openMethod);

            // Create the closed class which contains a List<string>

            Class closedClass = new Class();
            closedClass.Name = "ClosedClass";
            closedClass.Namespace = "Namespace";

            TypeSpecifier closedListType = TypeSpecifier.FromType<string>();

            Method closedMethod = new Method("ClosedMethod");
            closedMethod.ArgumentTypes.Add(closedListType);
            GraphUtil.ConnectExecPins(closedMethod.EntryNode.InitialExecutionPin, closedMethod.ReturnNode.ReturnPin);

            closedClass.Methods.Add(closedMethod);

            // Translate the classes

            ClassTranslator translator = new ClassTranslator();
            
            string openClassTranslated = translator.TranslateClass(openClass);

            string closedClassTranslated = translator.TranslateClass(closedClass);
        }
    }
}
