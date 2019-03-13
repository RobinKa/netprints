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
        /// Specifier for the type to cast to.
        /// </summary>
        [DataMember]
        public TypeSpecifier CastType
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Pin for the object to cast to another type.
        /// </summary>
        public NodeInputDataPin ObjectToCast
        {
            get { return InputDataPins[0]; }
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

        public ExplicitCastNode(Method method, TypeSpecifier castType)
            : base(method)
        {
            CastType = castType;

            AddInputDataPin("Object", TypeSpecifier.FromType<object>());
            AddOutputDataPin("CastObject", castType);
            AddInputExecPin("Exec");
            AddOutputExecPin("Success");
            AddOutputExecPin("Failure");
        }

        public override string ToString()
        {
            return $"Explicit Cast to {CastType.ShortName}";
        }
    }
}
