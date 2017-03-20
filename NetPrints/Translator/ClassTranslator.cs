using NetPrints.Core;
using System.Text;

namespace NetPrints.Translator
{
    public class ClassTranslator
    {
        private const string CLASS_TEMPLATE =
            @"namespace %Namespace%
            {
                %ClassModifiers%class %ClassName% : %SuperType%
                {
                    %Content%
                }
            }";

        private const string VARIABLE_TEMPLATE = "%VariableModifiers%%VariableType% %VariableName% { get; set; }";

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

            StringBuilder modifiers = new StringBuilder();

            if (c.Modifiers.HasFlag(ClassModifiers.Public))
            {
                modifiers.Append("public ");
            }

            if (c.Modifiers.HasFlag(ClassModifiers.Static))
            {
                modifiers.Append("static ");
            }

            if(c.Modifiers.HasFlag(ClassModifiers.Abstract))
            {
                modifiers.Append("abstract ");
            }

            if (c.Modifiers.HasFlag(ClassModifiers.Sealed))
            {
                modifiers.Append("sealed ");
            }

            return CLASS_TEMPLATE
                .Replace("%Namespace%", c.Namespace)
                .Replace("%ClassModifiers%", modifiers.ToString())
                .Replace("%ClassName%", c.Name)
                .Replace("%SuperType%", c.SuperType.FullName)
                .Replace("%Content%", content.ToString());
        }

        public string TranslateVariable(Variable variable)
        {
            StringBuilder modifiers = new StringBuilder();
            
            if (variable.Modifiers.HasFlag(VariableModifiers.Protected))
            {
                modifiers.Append("protected ");
            }
            else if (variable.Modifiers.HasFlag(VariableModifiers.Public))
            {
                modifiers.Append("public ");
            }
            else if (variable.Modifiers.HasFlag(VariableModifiers.Internal))
            {
                modifiers.Append("internal ");
            }

            if (variable.Modifiers.HasFlag(VariableModifiers.Static))
            {
                modifiers.Append("static ");
            }

            if (variable.Modifiers.HasFlag(VariableModifiers.ReadOnly))
            {
                modifiers.Append("readonly ");
            }

            if (variable.Modifiers.HasFlag(VariableModifiers.New))
            {
                modifiers.Append("new ");
            }

            if (variable.Modifiers.HasFlag(VariableModifiers.Const))
            {
                modifiers.Append("const ");
            }

            return VARIABLE_TEMPLATE
                .Replace("%VariableModifiers%", modifiers.ToString())
                .Replace("%VariableType%", variable.VariableType.FullName)
                .Replace("%VariableName%", variable.Name);
        }

        public string TranslateMethod(Method m)
        {
            return methodTranslator.Translate(m);
        }
    }
}
