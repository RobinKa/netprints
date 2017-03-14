using System;
using System.Collections.Generic;
using System.Text;

namespace NetPrints.Core
{
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

        public Variable(string name, Type variableType)
        {
            Name = name;
            VariableType = variableType;
        }
    }
}
