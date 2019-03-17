using NetPrints.Core;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Abstract base class for all node types.
    /// </summary>
    [DataContract]
    [KnownType(typeof(CallMethodNode))]
    [KnownType(typeof(EntryNode))]
    [KnownType(typeof(ForLoopNode))]
    [KnownType(typeof(IfElseNode))]
    [KnownType(typeof(LiteralNode))]
    [KnownType(typeof(ReturnNode))]
    [KnownType(typeof(VariableGetterNode))]
    [KnownType(typeof(VariableSetterNode))]
    [KnownType(typeof(ConstructorNode))]
    [KnownType(typeof(MakeDelegateNode))]
    [KnownType(typeof(TypeOfNode))]
    [KnownType(typeof(ExplicitCastNode))]
    [KnownType(typeof(RerouteNode))]
    [KnownType(typeof(MakeArrayNode))]
    public abstract class Node
    {
        /// <summary>
        /// Input data pins of this node.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<NodeInputDataPin> InputDataPins { get; private set; } = new ObservableRangeCollection<NodeInputDataPin>();

        /// <summary>
        /// Output data pins of this node.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<NodeOutputDataPin> OutputDataPins { get; private set; } = new ObservableRangeCollection<NodeOutputDataPin>();

        /// <summary>
        /// Input execution pins of this node.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<NodeInputExecPin> InputExecPins { get; private set; } = new ObservableRangeCollection<NodeInputExecPin>();

        /// <summary>
        /// Output execution pins of this node.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<NodeOutputExecPin> OutputExecPins { get; private set; } = new ObservableRangeCollection<NodeOutputExecPin>();

        /// <summary>
        /// Delegate for the event of a position change of a node.
        /// </summary>
        /// <param name="node">Node that changed position.</param>
        /// <param name="positionX">New position x value.</param>
        /// <param name="positionY">New position y value.</param>
        public delegate void NodePositionChangedDelegate(Node node, double positionX, double positionY);

        /// <summary>
        /// Called when this node's position changes.
        /// </summary>
        public event NodePositionChangedDelegate OnPositionChanged;

        /// <summary>
        /// Visual position x of this node.
        /// Triggers a call to OnPositionChange when set.
        /// </summary>
        [DataMember]
        public double PositionX
        {
            get => positionX;
            set
            {
                positionX = value;
                OnPositionChanged?.Invoke(this, positionX, positionY);
            }
        }

        /// <summary>
        /// Visual position y of this node.
        /// Triggers a call to OnPositionChange when set.
        /// </summary>
        [DataMember]
        public double PositionY
        {
            get => positionY;
            set
            {
                positionY = value;
                OnPositionChanged?.Invoke(this, positionX, positionY);
            }
        }
        
        private double positionX;
        private double positionY;

        /// <summary>
        /// Name of this node.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Whether this is a pure node (ie. one without any execution pins).
        /// These nodes will usually be executed when one of their output data
        /// pins is used in an execution node.
        /// </summary>
        public bool IsPure
        {
            get
            {
                return InputExecPins.Count == 0 && OutputExecPins.Count == 0;
            }
        }

        /// <summary>
        /// Method this node is contained in.
        /// </summary>
        [DataMember]
        public Method Method
        {
            get;
            private set;
        }

        public Node(Method method)
        {
            Method = method;
            method.Nodes.Add(this);

            Name = NetPrintsUtil.GetUniqueName(GetType().Name, method.Nodes.Select(n => n.Name).ToList());
        }
        
        public override string ToString()
        {
            return GraphUtil.SplitCamelCase(GetType().Name);
        }

        /// <summary>
        /// Adds an input data pin to this node.
        /// </summary>
        /// <param name="pinName">Name of the pin.</param>
        /// <param name="pinType">Specifier for the type of this pin.</param>
        protected void AddInputDataPin(string pinName, BaseType pinType)
        {
            InputDataPins.Add(new NodeInputDataPin(this, pinName, pinType));
        }

        /// <summary>
        /// Adds an output data pin to this node.
        /// </summary>
        /// <param name="pinName">Name of the pin.</param>
        /// <param name="pinType">Specifier for the type of this pin.</param>
        protected void AddOutputDataPin(string pinName, BaseType pinType)
        {
            OutputDataPins.Add(new NodeOutputDataPin(this, pinName, pinType));
        }

        /// <summary>
        /// Adds an input execution pin to this node.
        /// </summary>
        /// <param name="pinName">Name of the pin.</param>
        protected void AddInputExecPin(string pinName)
        {
            InputExecPins.Add(new NodeInputExecPin(this, pinName));
        }

        /// <summary>
        /// Adds an output execution pin to this node.
        /// </summary>
        /// <param name="pinName">Name of the pin.</param>
        protected void AddOutputExecPin(string pinName)
        {
            OutputExecPins.Add(new NodeOutputExecPin(this, pinName));
        }
    }
}
