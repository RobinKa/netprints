using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing an if / else expression.
    /// </summary>
    [DataContract]
    public class IfElseNode : Node
    {
        /// <summary>
        /// Pin that gets executed if the statement was true.
        /// </summary>
        public NodeOutputExecPin TruePin
        {
            get { return OutputExecPins[0]; }
        }

        /// <summary>
        /// Pin that gets executed if the statement was false.
        /// </summary>
        public NodeOutputExecPin FalsePin
        {
            get { return OutputExecPins[1]; }
        }

        /// <summary>
        /// Input execution pin that executes this node.
        /// </summary>
        public NodeInputExecPin ExecutionPin
        {
            get { return InputExecPins[0]; }
        }

        /// <summary>
        /// Input data pin for the condition of this node.
        /// Expects a boolean value.
        /// </summary>
        public NodeInputDataPin ConditionPin
        {
            get { return InputDataPins[0]; }
        }

        public IfElseNode(Method method)
            : base(method)
        {
            AddInputExecPin("Exec");

            AddInputDataPin("Condition", TypeSpecifier.FromType<bool>());

            AddOutputExecPin("True");
            AddOutputExecPin("False");
        }

        public override string ToString()
        {
            return "If Else";
        }
    }
}
