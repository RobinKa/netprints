using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class ConstructorNode : ExecNode
    {
        [DataMember]
        public Type ClassType
        {
            get;
            private set;
        }

        public IList<NodeInputDataPin> ArgumentPins
        {
            get { return InputDataPins; }
        }

        public ConstructorNode(Method method, Type classType, IEnumerable<Type> argumentTypes)
            : base(method)
        {
            ClassType = classType;

            foreach(Type argumentType in argumentTypes)
            {
                AddInputDataPin(argumentType.Name, argumentType);
            }
            
            AddOutputDataPin(classType.Name, classType);
        }

        public override string ToString()
        {
            return $"Construct New {ClassType}";
        }
    }
}
