using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing the creation of a delegate (method pointer).
    /// </summary>
    [DataContract]
    public class MakeDelegateNode : Node
    {
        /// <summary>
        /// Specifier describing the method the delegate is created for.
        /// </summary>
        [DataMember]
        public MethodSpecifier MethodSpecifier
        {
            get;
            private set;
        }
        
        /// <summary>
        /// The target this delegate is for ("this").
        /// Accessing this for static methods (IsFromStaticMethod==true)
        /// will throw an exception.
        /// </summary>
        public NodeInputDataPin TargetPin
        {
            get => InputDataPins[0];
        }

        /// <summary>
        /// Whether the delegate is for a static method.
        /// </summary>
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
