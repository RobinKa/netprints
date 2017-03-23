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
        public MethodSpecifier MethodSpecifier
        {
            get;
            private set;
        }
        
        public string MethodName
        {
            get => MethodSpecifier.Name;
        }
        
        public bool IsStatic
        {
            get => MethodSpecifier.Modifiers.HasFlag(MethodModifiers.Static);
        }
        
        public TypeSpecifier DeclaringType
        {
            get => MethodSpecifier.DeclaringType;
        }

        public IList<TypeSpecifier> ArgumentTypes
        {
            get => MethodSpecifier.Arguments;
        }

        public IList<TypeSpecifier> ReturnTypes
        {
            get => MethodSpecifier.ReturnTypes;
        }
        
        [DataMember]
        public IList<BaseType> GenericArgumentTypes
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
            get
            {
                if (IsStatic)
                {
                    return InputDataPins;
                }
                else
                {
                    // First pin is the target object, ignore it
                    return InputDataPins.Skip(1).ToList();
                }
            }
        }

        public CallMethodNode(Method method, MethodSpecifier methodSpecifier, 
            IList<BaseType> genericArgumentTypes = null)
            : base(method)
        {
            MethodSpecifier = methodSpecifier;

            // TODO: Check that genericArgumentTypes fullfils GenericArguments constraints
            if (MethodSpecifier.GenericArguments.Count > 0 && genericArgumentTypes != null
                || (genericArgumentTypes != null && 
                MethodSpecifier.GenericArguments.Count != genericArgumentTypes.Count))
            {
                throw new ArgumentException(nameof(genericArgumentTypes));
            }

            if (genericArgumentTypes == null)
            {
                GenericArgumentTypes = new List<BaseType>();
            }
            else
            {
                GenericArgumentTypes = genericArgumentTypes;
            }
            
            if (!IsStatic)
            {
                AddInputDataPin("Target", DeclaringType);
            }

            foreach(TypeSpecifier argumentType in ArgumentTypes)
            {
                AddInputDataPin(argumentType.ShortName, argumentType);
            }

            foreach(TypeSpecifier returnType in ReturnTypes)
            {
                AddOutputDataPin(returnType.ShortName, returnType);
            }
        }

        public override string ToString()
        {
            if (IsStatic)
            {
                return $"Call Static {DeclaringType} {MethodName}";
            }
            else
            {
                return $"Call {MethodName}";
            }
        }
    }
}
