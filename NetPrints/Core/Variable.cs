using System;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    /// <summary>
    /// Modifiers variables can have. Can be combined.
    /// </summary>
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

    /// <summary>
    /// Variable type. Contains common things for variables such as their type and their name.
    /// </summary>
    [DataContract]
    public class Variable
    {
        /// <summary>
        /// Specifier for the type of this variable.
        /// </summary>
        [DataMember]
        public TypeSpecifier VariableType
        {
            get;
            set;
        }

        /// <summary>
        /// Name of this variable without any prefixes.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Modifiers for this variable.
        /// </summary>
        [DataMember]
        public VariableModifiers Modifiers
        {
            get;
            set;
        } = VariableModifiers.Private;

        /// <summary>
        /// Creates a variable.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <param name="variableType">Specifier for the type of the variable.</param>
        public Variable(string name, TypeSpecifier variableType)
        {
            Name = name;
            VariableType = variableType;
        }
    }
}
