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
        public ConstructorSpecifier ConstructorSpecifier
        {
            get;
            private set;
        }
        
        public TypeSpecifier ClassType
        {
            get => ConstructorSpecifier.DeclaringType;
        }

        public IList<TypeSpecifier> ArgumentTypes
        {
            get => ConstructorSpecifier.Arguments;
        }

        public IList<NodeInputDataPin> ArgumentPins
        {
            get { return InputDataPins; }
        }

        public ConstructorNode(Method method, ConstructorSpecifier specifier)
            : base(method)
        {
            ConstructorSpecifier = specifier;

            foreach(TypeSpecifier argumentType in ArgumentTypes)
            {
                AddInputDataPin(argumentType.ShortName, argumentType);
            }
            
            AddOutputDataPin(ClassType.ShortName, ClassType);
        }

        public override string ToString()
        {
            return $"Construct New {ClassType}";
        }
    }
}
