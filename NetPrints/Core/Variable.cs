using System;
using System.Collections.Generic;
using System.Text;

namespace NetPrints.Core
{
    [Flags]
    public enum VariableModifiers
    {
        Private = 0,
        Public = 1,
        Protected = 2,
        Internal = 4,
        ReadOnly = 8,
        Const = 16,
        Static = 32,
        New = 64,
    }

    public class Variable
    {
        public Type VariableType
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public VariableModifiers Modifiers
        {
            get;
            set;
        } = VariableModifiers.Private;

        public Variable(string name, Type variableType)
        {
            Name = name;
            VariableType = variableType;
        }
    }
}
