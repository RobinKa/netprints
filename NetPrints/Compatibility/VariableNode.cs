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
            private set
            {
                oldVariableName = value.Name;
            }
        }

        private string oldVariableName = null;

        internal void FixOldVariable()
        {
            if (oldVariableName != null)
            {
                Variable = Method.Class.Variables.FirstOrDefault(newVar => newVar.Name == oldVariableName)?.Specifier;
            }
        }
    }
}
