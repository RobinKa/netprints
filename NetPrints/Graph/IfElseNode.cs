using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    [DataContract]
    public class IfElseNode : Node
    {
        public NodeOutputExecPin TruePin
        {
            get { return OutputExecPins[0]; }
        }

        public NodeOutputExecPin FalsePin
        {
            get { return OutputExecPins[1]; }
        }

        public NodeInputExecPin ExecutionPin
        {
            get { return InputExecPins[0]; }
        }

        public NodeInputDataPin ConditionPin
        {
            get { return InputDataPins[0]; }
        }

        public IfElseNode(Method method)
            : base(method)
        {
            AddInputExecPin("Exec");

            AddInputDataPin("Condition", typeof(bool));

            AddOutputExecPin("True");
            AddOutputExecPin("False");
        }
    }
}
