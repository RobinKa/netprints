using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using NetPrints.Core;

namespace NetPrints.Translator
{
    class ClassTranslator
    {
        private string classTemplate;
        private string variableTemplate;
        private string methodTemplate;
        
        public ClassTranslator()
        {
            classTemplate = File.ReadAllText("Class.template");
            variableTemplate = File.ReadAllText("Variable.template");
            methodTemplate = File.ReadAllText("Method.template");
        }

        public string TranslateClass(Class c)
        {
            StringBuilder content = new StringBuilder();

            foreach (Variable v in c.Attributes)
            {
                content.AppendLine(TranslateVariable(v));
            }

            foreach(Method m in c.Methods)
            {
                content.AppendLine(TranslateMethod(m));
            }

            return classTemplate
                .Replace("%Namespace", c.Namespace)
                .Replace("%ClassName%", c.Name)
                .Replace("%SuperType%", c.SuperType.FullName)
                .Replace("%Content%", content.ToString());
        }

        public string TranslateVariable(Variable variable)
        {
            return variableTemplate
                .Replace("%VariableType%", variable.VariableType.FullName)
                .Replace("%VariableName%", variable.Name);
        }

        public string TranslateMethod(Method m)
        {
            string methodContent = "";
            string returnType = "";
            string arguments = "";

            if(m.ReturnTypes.Length == 0)
            {
                returnType = "void";
            }
            else if(m.ReturnTypes.Length == 1)
            {
                returnType = m.ReturnTypes[0].FullName;
            }
            else
            {
                returnType = string.Concat("Tuple<", string.Join(",", m.ReturnTypes.Select(t => t.FullName)), ">");
            }

            return methodTemplate
                .Replace("%ReturnType%", returnType)
                .Replace("%MethodName%", m.Name)
                .Replace("%Arguments%", arguments)
                .Replace("%Content%", methodContent);
        }
    }
}
