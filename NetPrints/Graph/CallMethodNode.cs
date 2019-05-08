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
        private const string ExceptionPinName = "Exception";
        private const string CatchPinName = "Catch";

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

        /// <summary>
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
            get { return OutputDataPins.SingleOrDefault(p => p.Name == ExceptionPinName); }
        }

        /// <summary>
        /// Pin that gets executed when an exception is caught.
        /// </summary>
        public NodeOutputExecPin CatchPin
        {
            get { return OutputExecPins.SingleOrDefault(p => p.Name == CatchPinName); }
        }

        /// <summary>
        /// Whether this node has exception handling (try/catch).
        /// </summary>
        public bool HandlesExceptions
        {
            get => !IsPure && OutputExecPins.Any(p => p.Name == CatchPinName) && CatchPin.OutgoingPin != null;
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
            get => (OutputDataPins.Where(p => p.Name != ExceptionPinName)).ToList();
        }

        public CallMethodNode(NodeGraph graph, MethodSpecifier methodSpecifier,
            IList<BaseType> genericArgumentTypes = null)
            : base(graph)
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

            foreach (var argument in MethodSpecifier.Parameters)
            {
                AddInputDataPin(argument.Name, argument.Value);

                // Set default parameter value if set
                if (argument.HasExplicitDefaultValue)
                {
                    var newPin = InputDataPins.Last();
                    newPin.UsesExplicitDefaultValue = true;
                    newPin.ExplicitDefaultValue = argument.ExplicitDefaultValue;
                }
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
            AddOutputExecPin(CatchPinName);
            AddCatchPinChangedEvent();
        }

        private void AddCatchPinChangedEvent()
        {
            if (CatchPin != null)
            {
                // Add / remove exception pin when catch is connected / unconnected
                CatchPin.OutgoingPinChanged += (pin, oldPin, newPin) => UpdateExceptionPin();
            }
        }

        /// <summary>
        /// Adds or removes the exception output data pin depending on
        /// whether the catch pin is connected.
        /// </summary>
        private void UpdateExceptionPin()
        {
            if (CatchPin?.OutgoingPin == null && ExceptionPin != null)
            {
                GraphUtil.DisconnectOutputDataPin(ExceptionPin);
                OutputDataPins.Remove(ExceptionPin);
            }
            else if (CatchPin?.OutgoingPin != null && ExceptionPin == null)
            {
                AddOutputDataPin(ExceptionPinName, TypeSpecifier.FromType<Exception>());
            }
        }

        public override void OnMethodDeserialized()
        {
            base.OnMethodDeserialized();
            AddCatchPinChangedEvent();
            UpdateExceptionPin();
        }

        protected override void SetPurity(bool pure)
        {
            base.SetPurity(pure);

            if (pure)
            {
                // Remove catch pin. Exception pin gets automatically removed because
                // of its pin changed ev ent.
                GraphUtil.DisconnectOutputExecPin(CatchPin);
                OutputExecPins.Remove(CatchPin);
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
            for (int i = 0; i < MethodSpecifier.Parameters.Count; i++)
            {
                BaseType type = MethodSpecifier.Parameters[i];

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
