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
        public TypeSpecifier ClassType
        {
            get;
            private set;
        }

        public IList<NodeInputDataPin> ArgumentPins
        {
            get { return InputDataPins; }
        }

        public ConstructorNode(Method method, TypeSpecifier classType, IEnumerable<TypeSpecifier> argumentTypes)
            : base(method)
        {
            ClassType = classType;

            foreach(TypeSpecifier argumentType in argumentTypes)
            {
                AddInputDataPin(argumentType.ShortName, argumentType);
            }
            
            AddOutputDataPin(classType.ShortName, classType);
        }

        public override string ToString()
        {
            return $"Construct New {ClassType}";
        }
    }
}
