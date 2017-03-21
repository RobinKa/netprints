using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class CallMethodNode : ExecNode
    {
        [DataMember]
        public string MethodName
        {
            get;
            private set;
        }

        [DataMember]
        public TypeSpecifier TargetType
        {
            get;
            private set;
        }

        public NodeInputDataPin TargetPin
        {
            get { return InputDataPins[0]; }
        }

        public IList<NodeInputDataPin> ArgumentPins
        {
            get { return InputDataPins.Skip(1).ToList(); }
        }

        public CallMethodNode(Method method, TypeSpecifier targetType, string methodName, IEnumerable<TypeSpecifier> inputTypes, IEnumerable<TypeSpecifier> outputTypes)
            : base(method)
        {
            MethodName = methodName;
            TargetType = targetType;

            AddInputDataPin("Target", targetType);
            
            foreach(TypeSpecifier inputType in inputTypes)
            {
                AddInputDataPin(inputType.ShortName, inputType);
            }

            foreach(TypeSpecifier outputType in outputTypes)
            {
                AddOutputDataPin(outputType.ShortName, outputType);
            }
        }

        public override string ToString()
        {
            return $"Call {MethodName}";
        }
    }
}
