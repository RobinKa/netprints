using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using NetPrints.Core;

namespace NetPrints.Translator
{
    public class ClassTranslator
    {
        private const string CLASS_TEMPLATE =
            @"namespace %Namespace%
            {
                public class %ClassName% : %SuperType%
                {
                    %Content%
                }
            }";

        private const string VARIABLE_TEMPLATE = "public %VariableType% %VariableName% { get; set; }";

        private MethodTranslator methodTranslator = new MethodTranslator();
        
        public ClassTranslator()
        {

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

            return CLASS_TEMPLATE
                .Replace("%Namespace%", c.Namespace)
                .Replace("%ClassName%", c.Name)
                .Replace("%SuperType%", c.SuperType.FullName)
                .Replace("%Content%", content.ToString());
        }

        public string TranslateVariable(Variable variable)
        {
            return VARIABLE_TEMPLATE
                .Replace("%VariableType%", variable.VariableType.FullName)
                .Replace("%VariableName%", variable.Name);
        }

        public string TranslateMethod(Method m)
        {
            return methodTranslator.Translate(m);
        }
    }
}
