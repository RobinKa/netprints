using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Abstract class for variable nodes.
    /// </summary>
    [DataContract]
    [KnownType(typeof(VariableGetterNode))]
    [KnownType(typeof(VariableSetterNode))]
    public abstract partial class VariableNode : Node
    {
        /// <summary>
        /// Target object of this variable node.
        /// Can be null for local variables.
        /// </summary>
        public NodeInputDataPin TargetPin
        {
            get { return !IsLocalVariable && !IsStatic ? InputDataPins[0] : null; }
        }

        /// <summary>
        /// Pin that outputs the value of the variable.
        /// </summary>
        public NodeOutputDataPin ValuePin
        {
            get { return OutputDataPins[0]; }
        }

        /// <summary>
        /// Whether the variable is a local variable.
        /// </summary>
        public bool IsLocalVariable => TargetType is null;

        /// <summary>
        /// Name of this variable.
        /// </summary>
        public string VariableName { get => Variable.Name; }

        /// <summary>
        /// Specifier for the type of the target object.
        /// </summary>
        public TypeSpecifier TargetType { get => Variable.DeclaringType; }

        /// <summary>
        /// Whether the variable is static.
        /// </summary>
        public bool IsStatic
        {
            get => Variable.Modifiers.HasFlag(VariableModifiers.Static);
        }

        /// <summary>
        /// Whether this variable node is for an indexer (eg. dict["key"]).
        /// </summary>
        public bool IsIndexer
        {
            get => Variable.Name == "this[]";
        }

        /// <summary>
        /// Specifier for the type of the index.
        /// </summary>
        public BaseType IndexType
        {
            // TODO: Get indexer type
            get => IsIndexer ? TypeSpecifier.FromType<object>() : null;
        }

        /// <summary>
        /// Data pin for the indexer.
        /// </summary>
        public NodeInputDataPin IndexPin
        {
            get => IsIndexer ? InputDataPins[1] : null;
        }

        /// <summary>
        /// Specifier for the underlying variable.
        /// </summary>
        [DataMember(Name = "FieldOrProperty")]
        public VariableSpecifier Variable { get; private set; }

        protected VariableNode(NodeGraph graph, VariableSpecifier variable)
            : base(graph)
        {
            Variable = variable;

            // Add target input pin if not local or static
            if (!IsLocalVariable && !Variable.Modifiers.HasFlag(VariableModifiers.Static))
            {
                AddInputDataPin("Target", TargetType);
            }

            if (IsIndexer)
            {
                AddInputDataPin("Index", IndexType);
            }

            AddOutputDataPin(Variable.Type.ShortName, Variable.Type);
        }
    }
}
