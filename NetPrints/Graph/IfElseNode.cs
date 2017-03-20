using NetPrints.Core;
using System.Runtime.Serialization;

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
