using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing an exception throw.
    /// </summary>
    [DataContract]
    public class ThrowNode : Node
    {
        /// <summary>
        /// Pin for the exception to throw.
        /// </summary>
        public NodeInputDataPin ExceptionPin
        {
            get { return InputDataPins[0]; }
        }

        public ThrowNode(Method method)
            : base(method)
        {
            AddInputExecPin("Exec");
            AddInputDataPin("Exception", TypeSpecifier.FromType<Exception>());
        }

        public override string ToString()
        {
            return $"Throw Exception";
        }
    }
}
