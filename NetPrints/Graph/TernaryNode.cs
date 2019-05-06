using NetPrints.Core;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing a ternary operation.
    /// </summary>
    [DataContract]
    public class TernaryNode : ExecNode
    {
        public override bool CanSetPure
        {
            get => true;
        }

        /// <summary>
        /// Pin for the object to choose when the condition was true.
        /// </summary>
        public NodeInputDataPin TrueObjectPin
        {
            get { return InputDataPins[0]; }
        }

        /// <summary>
        /// Pin for the object to choose when the condition was false.
        /// </summary>
        public NodeInputDataPin FalseObjectPin
        {
            get { return InputDataPins[1]; }
        }

        /// <summary>
        /// Pin for the selection condition.
        /// </summary>
        public NodeInputDataPin ConditionPin
        {
            get { return InputDataPins[2]; }
        }

        /// <summary>
        /// Input type pin for the type to select.
        /// </summary>
        public NodeInputTypePin TypePin
        {
            get { return InputTypePins[0]; }
        }

        /// <summary>
        /// Pin that holds the selected object.
        /// </summary>
        public NodeOutputDataPin OutputObjectPin
        {
            get { return OutputDataPins[0]; }
        }

        /// <summary>
        /// Type to cast to. Inferred from input type pin.
        /// </summary>
        public BaseType Type
        {
            get => TypePin.InferredType?.Value ?? TypeSpecifier.FromType<object>();
        }

        public TernaryNode(NodeGraph graph)
            : base(graph)
        {
            AddInputTypePin("Type");
            AddInputDataPin("True", TypeSpecifier.FromType<object>());
            AddInputDataPin("False", TypeSpecifier.FromType<object>());
            AddInputDataPin("Condition", TypeSpecifier.FromType<bool>());
            AddOutputDataPin("Output", Type);
        }

        protected override void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            base.OnInputTypeChanged(sender, eventArgs);

            TrueObjectPin.PinType.Value = Type;
            FalseObjectPin.PinType.Value = Type;
            OutputObjectPin.PinType.Value = Type;
        }

        public override string ToString()
        {
            return $"Ternary {Type.ShortName}";
        }
    }
}
