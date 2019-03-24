using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing a constructor call.
    /// </summary>
    [DataContract]
    public class ConstructorNode : ExecNode
    {
        public override bool CanSetPure
        {
            get => true;
        }

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
        public BaseType ClassType
        {
            get => OutputDataPins[0].PinType.Value;
        }

        /// <summary>
        /// List of type specifiers the constructor takes.
        /// </summary>
        public IReadOnlyList<BaseType> ArgumentTypes
        {
            get => ArgumentPins.Select(p => p.PinType.Value).ToList();
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

            // Add type pins for each generic arguments of the type being constructed.
            foreach (var genericArg in ConstructorSpecifier.DeclaringType.GenericArguments.OfType<GenericType>())
            {
                AddInputTypePin(genericArg.Name);
            }

            foreach (TypeSpecifier argumentType in ConstructorSpecifier.Arguments)
            {
                AddInputDataPin(argumentType.ShortName, argumentType);
            }
            
            AddOutputDataPin(ConstructorSpecifier.DeclaringType.ShortName, ConstructorSpecifier.DeclaringType);

            // TODO: Set the correct types to begin with.
            UpdateTypes();
        }

        protected override void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            base.OnInputTypeChanged(sender, eventArgs);
            UpdateTypes();
        }

        private void UpdateTypes()
        {
            // Construct data input
            for (int i = 0; i < ConstructorSpecifier.Arguments.Count; i++)
            {
                BaseType type = ConstructorSpecifier.Arguments[i];

                // Construct type with generic arguments replaced by our input type pins
                BaseType constructedType = GenericsHelper.ConstructWithTypePins(type, InputTypePins);

                if (InputDataPins[i].PinType.Value != constructedType)
                {
                    InputDataPins[i].PinType.Value = constructedType;
                }
            }

            // Construct data output
            {
                BaseType constructedType = GenericsHelper.ConstructWithTypePins(ConstructorSpecifier.DeclaringType, InputTypePins);
                OutputDataPins[0].PinType.Value = constructedType;
            }
        }

        public override string ToString()
        {
            return $"Construct {ClassType.ShortName}";
        }
    }
}
