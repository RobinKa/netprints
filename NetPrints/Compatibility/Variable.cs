using System;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    [DataContract(Name = "Variable")]
    [Obsolete]
    public class OldVariable
    {
        [Obsolete]
        [OnDeserialized]
        private void FixVisibility(StreamingContext context)
        {
            // Set new visibility from old modifiers
            if (Modifiers.HasFlag(VariableModifiers.Public))
            {
                Modifiers &= ~(VariableModifiers.Public);
                Visibility = MemberVisibility.Public;
            }
            else if (Modifiers.HasFlag(VariableModifiers.Protected))
            {
                Modifiers &= ~(VariableModifiers.Protected);
                Visibility = MemberVisibility.Protected;
            }
            else if (Modifiers.HasFlag(VariableModifiers.Internal))
            {
                Modifiers &= ~(VariableModifiers.Internal);
                Visibility = MemberVisibility.Internal;
            }
            else if (Visibility == MemberVisibility.Invalid)
            {
                Visibility = MemberVisibility.Private;
            }
        }

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
        /// Type that contains this variable.
        /// </summary>
        [DataMember]
        public BaseType DeclaringType
        {
            get;
            private set;
        }

        /// <summary>
        /// Modifiers for this variable.
        /// </summary>
        [DataMember]
        public VariableModifiers Modifiers
        {
            get;
            set;
        }

        /// <summary>
        /// Visibility of this variable.
        /// </summary>
        [DataMember]
        public MemberVisibility Visibility { get; set; } = MemberVisibility.Private;

        /// <summary>
        /// Creates a variable.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <param name="variableType">Specifier for the type of the variable.</param>
        public OldVariable(string name, TypeSpecifier variableType, BaseType declaringType)
        {
            Name = name;
            VariableType = variableType;
            DeclaringType = declaringType;
        }
    }
}
