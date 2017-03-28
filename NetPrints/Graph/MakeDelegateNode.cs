using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class MakeDelegateNode : Node
    {
        [DataMember]
        public MethodSpecifier MethodSpecifier
        {
            get;
            private set;
        }
        
        public NodeInputDataPin TargetPin
        {
            get => InputDataPins[0];
        }

        public bool IsFromStaticMethod
        {
            get => MethodSpecifier.Modifiers.HasFlag(MethodModifiers.Static);
        }

        public MakeDelegateNode(Method method, MethodSpecifier methodSpecifier)
            : base(method)
        {
            MethodSpecifier = methodSpecifier;
            
            if (!IsFromStaticMethod)
            {
                AddInputDataPin("Target", methodSpecifier.DeclaringType);
            }

            TypeSpecifier delegateType;

            if (methodSpecifier.ReturnTypes.Count == 0)
            {
                delegateType = new TypeSpecifier("System.Action", false, false, methodSpecifier.Arguments);
            }
            else if(methodSpecifier.ReturnTypes.Count == 1)
            {
                delegateType = new TypeSpecifier("System.Func", false, false, methodSpecifier.Arguments.Concat(methodSpecifier.ReturnTypes).ToList());
            }
            else
            {
                throw new NotImplementedException("Only 0 and 1 return types are supported right now.");
            }

            AddOutputDataPin(delegateType.ShortName, delegateType);
        }

        public override string ToString()
        {
            if(IsFromStaticMethod)
            {
                return $"Make Delegate from {MethodSpecifier.DeclaringType} {MethodSpecifier.Name}";
            }
            else
            {
                return $"Make Delegate from {MethodSpecifier.Name}";
            }
        }
    }
}
