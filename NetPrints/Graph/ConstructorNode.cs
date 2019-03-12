using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing a constructor call.
    /// </summary>
    [DataContract]
    public class ConstructorNode : ExecNode
    {
        /// <summary>
        /// Specifier for the constructor.
        /// </summary>
        [DataMember]
        public ConstructorSpecifier ConstructorSpecifier
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Specifier for the type this constructor creates.
        /// </summary>
        public TypeSpecifier ClassType
        {
            get => ConstructorSpecifier.DeclaringType;
        }

        /// <summary>
        /// List of specifiers for the types this constructor takes.
        /// </summary>
        public IList<TypeSpecifier> ArgumentTypes
        {
            get => ConstructorSpecifier.Arguments;
        }

        /// <summary>
        /// List of node pins, one for each argument the constructor takes.
        /// </summary>
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
