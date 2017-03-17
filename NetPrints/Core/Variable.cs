using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

    [DataContract]
    public class Variable
    {
        [DataMember]
        public Type VariableType
        {
            get;
            set;
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
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
