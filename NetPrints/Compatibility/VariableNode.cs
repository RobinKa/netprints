using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NetPrints.Core;

namespace NetPrints.Graph
{
    public partial class VariableNode
    {
        [DataMember(Name = "Variable", IsRequired = false, EmitDefaultValue = false)]
        [Obsolete]
        public OldVariable OldVariable
        {
            get => null;
            private set => oldVariable = value;
        }

        [DataMember(Name = "TargetType", IsRequired = false, EmitDefaultValue = false)]
        [Obsolete]
        public TypeSpecifier OldTargetType
        {
            get => null;
            private set => oldTargetType = value;
        }

        private OldVariable oldVariable = null;
        private TypeSpecifier oldTargetType = null;

        internal void FixOldVariable()
        {
            if (oldVariable != null)
            {
                // Try to find the variable in the class
                Variable = Method.Class.Variables.FirstOrDefault(newVar => newVar.Class.FullName == oldTargetType.Name && newVar.Name == oldVariable.Name)?.Specifier;

                // If it was not found recreate it
                if (Variable is null)
                {
                    Variable = new VariableSpecifier(oldVariable.Name, oldVariable.VariableType, oldVariable.Visibility,
                        oldVariable.Visibility, oldTargetType, oldVariable.Modifiers)
                    {
                        Visibility = oldVariable.Visibility
                    };
                }
            }
        }
    }
}
