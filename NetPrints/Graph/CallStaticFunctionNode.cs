using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class CallStaticFunctionNode : ExecNode
    {
        [DataMember]
        public string MethodName
        {
            get;
            private set;
        }

        [DataMember]
        public TypeSpecifier ClassType
        {
            get;
            private set;
        }

        public IList<NodeInputDataPin> ArgumentPins
        {
            get { return InputDataPins; }
        }

        public CallStaticFunctionNode(Method method, TypeSpecifier classType, string methodName, IEnumerable<TypeSpecifier> inputTypes, IEnumerable<TypeSpecifier> outputTypes)
            : base(method)
        {
            ClassType = classType;
            MethodName = methodName;
            
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
            return $"Call Static {ClassType} {MethodName}";
        }
    }
}
