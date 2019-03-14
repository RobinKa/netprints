using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing a method call.
    /// </summary>
    [DataContract]
    public class CallMethodNode : ExecNode
    {
        /// <summary>
        /// Specifier for the method to call.
        /// </summary>
        [DataMember]
        public MethodSpecifier MethodSpecifier
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Name of the method without any prefixes.
        /// </summary>
        public string MethodName
        {
            get => MethodSpecifier.Name;
        }
        
        /// <summary>
        /// Whether the method is static.
        /// </summary>
        public bool IsStatic
        {
            get => MethodSpecifier.Modifiers.HasFlag(MethodModifiers.Static);
        }
        
        /// <summary>
        /// Specifier for the type the method is contained in.
        /// </summary>
        public TypeSpecifier DeclaringType
        {
            get => MethodSpecifier.DeclaringType;
        }

        /// <summary>
        /// List of type specifiers the method takes.
        /// </summary>
        public IList<BaseType> ArgumentTypes
        {
            get => MethodSpecifier.Arguments;
        }

        /// <summary>
        /// List of type specifiers the method returns.
        /// </summary>
        public IList<BaseType> ReturnTypes
        {
            get => MethodSpecifier.ReturnTypes;
        }
        
        /// <summary>
        /// List of generic arguments the method takes.
        /// </summary>
        [DataMember]
        public IList<BaseType> GenericArgumentTypes
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Target ("this") to call the method on.
        /// </summary>
        public NodeInputDataPin TargetPin
        {
            get { return InputDataPins[0]; }
        }

        /// <summary>
        /// Pin that holds the exception when catch is executed.
        /// </summary>
        public NodeOutputDataPin ExceptionPin
        {
            get { return OutputDataPins[0]; }
        }

        /// <summary>
        /// Pin that gets executed when an exception is caught.
        /// </summary>
        public NodeOutputExecPin CatchPin
        {
            get { return OutputExecPins[1]; }
        }

        /// <summary>
        /// List of node pins, one for each argument the method takes.
        /// </summary>
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

        /// <summary>
        /// List of node pins, one for each value the node's method returns (ie. no exception).
        /// </summary>
        public IList<NodeOutputDataPin> ReturnValuePins
        {
            get =>  OutputDataPins.Skip(1).ToList();
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

            AddOutputDataPin("Exception", TypeSpecifier.FromType<Exception>());
            AddOutputExecPin("Catch");

            foreach(BaseType argumentType in ArgumentTypes)
            {
                AddInputDataPin(argumentType.ShortName, argumentType);
            }

            foreach(BaseType returnType in ReturnTypes)
            {
                AddOutputDataPin(returnType.ShortName, returnType);
            }
        }

        public override string ToString()
        {
            string s = IsStatic ? "Static " : "";

            if (OperatorUtil.TryGetOperatorInfo(MethodSpecifier, out OperatorInfo operatorInfo))
            {
                s += $"Operator {operatorInfo.DisplayName}";
            }
            else
            {
                s += $"Call {MethodName}";
            }

            if (GenericArgumentTypes.Count > 0)
            {
                s += $"<{string.Join(", ", GenericArgumentTypes.Select(type => type.ShortName))}>";
            }

            return s;
        }
    }
}
