using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using NetPrints.Core;

namespace NetPrints.Graph
{
    public partial class VariableNode
    {
        [DataMember(Name = "Variable")]
        [Obsolete]
        public OldVariable OldVariable { get; private set; }
    }
}
