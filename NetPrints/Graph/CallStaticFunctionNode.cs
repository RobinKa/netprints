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
        public string ClassName
        {
            get;
            private set;
        }

        public IList<NodeInputDataPin> ArgumentPins
        {
            get { return InputDataPins; }
        }

        public CallStaticFunctionNode(Method method, string className, string methodName, IEnumerable<Type> inputTypes, IEnumerable<Type> outputTypes)
            : base(method)
        {
            ClassName = className;
            MethodName = methodName;
            
            foreach(Type inputType in inputTypes)
            {
                AddInputDataPin(inputType.Name, inputType);
            }

            foreach(Type outputType in outputTypes)
            {
                AddOutputDataPin(outputType.Name, outputType);
            }
        }

        public override string ToString()
        {
            return $"Call Static {ClassName} {MethodName}";
        }
    }
}
