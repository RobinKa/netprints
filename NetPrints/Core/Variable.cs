using System;
using System.Runtime.Serialization;

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
        public TypeSpecifier VariableType
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

        public Variable(string name, TypeSpecifier variableType)
        {
            Name = name;
            VariableType = variableType;
        }
    }
}
