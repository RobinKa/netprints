using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NetPrints.Graph
{
    [DataContract]
    public class TypeNode : Node
    {
        [DataMember]
        public BaseType Type
        {
            get;
            private set;
        }

        public TypeNode(Method method, BaseType type)
            : base(method)
        {
            Type = type;
            
            // Add type pins for each generic argument of the literal type
            // and monitor them for changes to reconstruct the actual pin types.
            if (Type is TypeSpecifier typeSpecifier)
            {
                foreach (var genericArg in typeSpecifier.GenericArguments.OfType<GenericType>())
                {
                    AddInputTypePin(genericArg.Name);
                }
            }

            AddOutputTypePin("OutputType", () =>
            {
                return GenericsHelper.ConstructWithTypePins(Type, InputTypePins);
            });
        }
    }
}
