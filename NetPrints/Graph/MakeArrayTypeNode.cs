using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class MakeArrayTypeNode : Node
    {
        [DataMember]
        private ObservableValue<BaseType> arrayType;

        public MakeArrayTypeNode(NodeGraph graph)
            : base(graph)
        {
            AddInputTypePin("ElementType");

            arrayType = new ObservableValue<BaseType>(GetArrayType());

            AddOutputTypePin("ArrayType", arrayType);
        }

        protected override void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            base.OnInputTypeChanged(sender, eventArgs);

            // Set the type of the output type pin by constructing
            // the type of this node with the input type pins.
            arrayType.Value = GetArrayType();
        }

        private BaseType GetArrayType()
        {
            var elementType = InputTypePins[0].InferredType?.Value ?? TypeSpecifier.FromType<object>();
            return new TypeSpecifier(elementType.Name + "[]", false, false, null);
        }

        public override string ToString()
        {
            return arrayType.Value.ShortName;
        }
    }
}
