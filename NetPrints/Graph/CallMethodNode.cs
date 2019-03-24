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
        public override bool CanSetPure
        {
            get => true;
        }

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
        /// Name of the method with generic arguments fully expanded as it
        /// would appear in code. (eg. SomeMethod&lt;System.Object, System.Int32&gt;).
        /// </summary>
        public string BoundMethodName
        {
            get
            {
                string boundName = MethodSpecifier.Name;

                if (InputTypePins.Count > 0)
                {
                    boundName += $"<{string.Join(",", InputTypePins.Select(p => p.InferredType?.Value?.FullCodeName ?? p.Name))}>";
                }

                return boundName;
            }
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
        public IReadOnlyList<BaseType> ArgumentTypes
        {
            get => InputDataPins.Select(p => p.PinType.Value).ToList();
        }

        // <summary>
        /// List of named type specifiers the method takes.
        /// </summary>
        public IReadOnlyList<Named<BaseType>> Arguments
        {
            get => InputDataPins.Select(p => new Named<BaseType>(p.Name, p.PinType.Value)).ToList();
        }

        /// <summary>
        /// List of type specifiers the method returns.
        /// </summary>
        public IReadOnlyList<BaseType> ReturnTypes
        {
            get => OutputDataPins.Select(p => p.PinType.Value).ToList();
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
            get { return OutputDataPins.Single(p => p.Name == "Exception"); }
        }

        /// <summary>
        /// Pin that gets executed when an exception is caught.
        /// </summary>
        public NodeOutputExecPin CatchPin
        {
            get { return OutputExecPins.Single(p => p.Name == "Catch"); }
        }
        
        /// <summary>
        /// Whether this node has exception handling (try/catch).
        /// </summary>
        public bool HandlesExceptions
        {
            get => !IsPure && OutputExecPins.Any(p => p.Name == "Catch") && CatchPin.OutgoingPin != null;
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
            get => (OutputDataPins.Where(p => p.Name != "Exception")).ToList();
        }

        public CallMethodNode(Method method, MethodSpecifier methodSpecifier, 
            IList<BaseType> genericArgumentTypes = null)
            : base(method)
        {
            MethodSpecifier = methodSpecifier;

            // Add type pins for each generic argument of the method type parameters.
            foreach (var genericArg in MethodSpecifier.GenericArguments.OfType<GenericType>())
            {
                AddInputTypePin(genericArg.Name);
            }
            
            if (!IsStatic)
            {
                AddInputDataPin("Target", DeclaringType);
            }

            AddExceptionPins();

            foreach (Named<BaseType> argument in MethodSpecifier.Arguments)
            {
                AddInputDataPin(argument.Name, argument.Value);
            }

            foreach (BaseType returnType in MethodSpecifier.ReturnTypes)
            {
                AddOutputDataPin(returnType.ShortName, returnType);
            }

            // TODO: Set the correct types to begin with.
            UpdateTypes();
        }

        private void AddExceptionPins()
        {
            AddOutputDataPin("Exception", TypeSpecifier.FromType<Exception>());
            AddOutputExecPin("Catch");
        }

        protected override void SetPurity(bool pure)
        {
            base.SetPurity(pure);

            if (pure)
            {
                GraphUtil.DisconnectOutputExecPin(CatchPin);
                OutputExecPins.Remove(CatchPin);

                GraphUtil.DisconnectOutputDataPin(ExceptionPin);
                OutputDataPins.Remove(ExceptionPin);
            }
            else
            {
                AddExceptionPins();
            }
        }

        protected override void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            base.OnInputTypeChanged(sender, eventArgs);

            UpdateTypes();
        }

        private void UpdateTypes()
        {
            for (int i = 0; i < MethodSpecifier.Arguments.Count; i++)
            {
                BaseType type = MethodSpecifier.Arguments[i];

                // Construct type with generic arguments replaced by our input type pins
                BaseType constructedType = GenericsHelper.ConstructWithTypePins(type, InputTypePins);

                if (ArgumentPins[i].PinType.Value != constructedType)
                {
                    ArgumentPins[i].PinType.Value = constructedType;
                }
            }

            for (int i = 0; i < MethodSpecifier.ReturnTypes.Count; i++)
            {
                BaseType type = MethodSpecifier.ReturnTypes[i];

                // Construct type with generic arguments replaced by our input type pins
                BaseType constructedType = GenericsHelper.ConstructWithTypePins(type, InputTypePins);

                // +1 because the first pin is the exception pin
                if (ReturnValuePins[i].PinType.Value != constructedType)
                {
                    ReturnValuePins[i].PinType.Value = constructedType;
                }
            }
        }

        public override string ToString()
        {
            if (OperatorUtil.TryGetOperatorInfo(MethodSpecifier, out OperatorInfo operatorInfo))
            {
                return $"Operator {operatorInfo.DisplayName}";
            }
            else
            {
                string s = "";

                if (IsStatic)
                {
                    s += $"{MethodSpecifier.DeclaringType.ShortName}.";
                }

                return s + MethodSpecifier.Name;
            }
        }
    }
}
