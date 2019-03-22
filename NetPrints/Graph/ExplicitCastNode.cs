using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing an explicit type cast.
    /// </summary>
    [DataContract]
    public class ExplicitCastNode : Node
    {        
        /// <summary>
        /// Pin for the object to cast to another type.
        /// </summary>
        public NodeInputDataPin ObjectToCast
        {
            get { return InputDataPins[0]; }
        }

        public NodeInputTypePin CastTypePin
        {
            get { return InputTypePins[0]; }
        }

        /// <summary>
        /// Pin that holds the cast object.
        /// </summary>
        public NodeOutputDataPin CastPin
        {
            get { return OutputDataPins[0]; }
        }

        /// <summary>
        /// Pin that gets executed when the cast succeeded.
        /// </summary>
        public NodeOutputExecPin CastSuccessPin
        {
            get { return OutputExecPins[0]; }
        }

        /// <summary>
        /// Pin that gets executed when the cast failed.
        /// </summary>
        public NodeOutputExecPin CastFailedPin
        {
            get { return OutputExecPins[1]; }
        }

        /// <summary>
        /// Type to cast to. Inferred from input type pin.
        /// </summary>
        public BaseType CastType
        {
            get { return CastTypePin.InferredType?.Value ?? TypeSpecifier.FromType<object>(); }
        }

        public ExplicitCastNode(Method method)
            : base(method)
        {
            AddInputTypePin("Type");
            AddInputDataPin("Object", TypeSpecifier.FromType<object>());
            AddOutputDataPin("CastObject", CastType);
            AddInputExecPin("Exec");
            AddOutputExecPin("Success");
            AddOutputExecPin("Failure");
        }

        protected override void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            base.OnInputTypeChanged(sender, eventArgs);

            CastPin.PinType.Value = CastType;
        }

        public override string ToString()
        {
            return $"Explicit Cast to {CastType.ShortName}";
        }
    }
}
