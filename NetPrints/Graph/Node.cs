﻿using NetPrints.Base;
using NetPrints.Core;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Abstract base class for all node types.
    /// </summary>
    [DataContract]
    [KnownType(typeof(CallMethodNode))]
    [KnownType(typeof(MethodEntryNode))]
    [KnownType(typeof(ConstructorEntryNode))]
    [KnownType(typeof(ForLoopNode))]
    [KnownType(typeof(IfElseNode))]
    [KnownType(typeof(LiteralNode))]
    [KnownType(typeof(ReturnNode))]
    [KnownType(typeof(ClassReturnNode))]
    [KnownType(typeof(VariableGetterNode))]
    [KnownType(typeof(VariableSetterNode))]
    [KnownType(typeof(ConstructorNode))]
    [KnownType(typeof(MakeDelegateNode))]
    [KnownType(typeof(TypeOfNode))]
    [KnownType(typeof(ExplicitCastNode))]
    [KnownType(typeof(RerouteNode))]
    [KnownType(typeof(MakeArrayNode))]
    [KnownType(typeof(TypeNode))]
    [KnownType(typeof(MakeArrayTypeNode))]
    [KnownType(typeof(ThrowNode))]
    [KnownType(typeof(AwaitNode))]
    [KnownType(typeof(TernaryNode))]
    [KnownType(typeof(TypeReturnNode))]
    [KnownType(typeof(DefaultNode))]
    [AddINotifyPropertyChangedInterface]
    public abstract class Node : INode
    {
        [DataMember]
        public ObservableRangeCollection<INodePin> Pins { get; private set; } = new ObservableRangeCollection<INodePin>();

        /// <summary>
        /// Input data pins of this node.
        /// </summary>

        public IObservableCollectionView<NodeInputDataPin> InputDataPins { get; private set; }

        /// <summary>
        /// Output data pins of this node.
        /// </summary>
        public IObservableCollectionView<NodeOutputDataPin> OutputDataPins { get; private set; }

        /// <summary>
        /// Input execution pins of this node.
        /// </summary>
        public IObservableCollectionView<NodeInputExecPin> InputExecPins { get; private set; }

        /// <summary>
        /// Output execution pins of this node.
        /// </summary>
        public IObservableCollectionView<NodeOutputExecPin> OutputExecPins { get; private set; }

        /// <summary>
        /// Input type pins of this node.
        /// </summary>
        public IObservableCollectionView<NodeInputTypePin> InputTypePins { get; private set; }

        /// <summary>
        /// Output type pins of this node.
        /// </summary>
        public IObservableCollectionView<NodeOutputTypePin> OutputTypePins { get; private set; }

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
            set
            {
                if (!CanSetPure)
                {
                    throw new InvalidOperationException("Can't set purity of this node.");
                }

                if (IsPure != value)
                {
                    SetPurity(value);
                }

                Debug.Assert(value == IsPure, "Purity could not be set correctly.");
            }
        }

        public virtual bool CanSetPure
        {
            get => false;
        }

        protected virtual void SetPurity(bool pure)
        {
        }

        /// <summary>
        /// Method graph this node is contained in.
        /// Null if the graph is not a MethodGraph.
        /// </summary>
        public MethodGraph MethodGraph
        {
            get => Graph as MethodGraph;
        }

        /// <summary>
        /// Graph this node is contained in.
        /// </summary>
        [DataMember]
        public NodeGraph Graph
        {
            get;
            private set;
        }

        INodeGraph INode.Graph => Graph;

        protected Node(NodeGraph graph)
        {
            SetupPinViews();

            Name = NetPrintsUtil.GetUniqueName(GetType().Name, graph.Nodes.Select(n => n.Name).ToList());

            Graph = graph;
            Graph.Nodes.Add(this);
        }

        private void SetupPinViews()
        {
            static bool isType<T>(object x) => x is T;

            InputDataPins = new FilteredObservableCollection<NodeInputDataPin, INodePin>(Pins, isType<NodeInputDataPin>);
            OutputDataPins = new FilteredObservableCollection<NodeOutputDataPin, INodePin>(Pins, isType<NodeOutputDataPin>);
            InputExecPins = new FilteredObservableCollection<NodeInputExecPin, INodePin>(Pins, isType<NodeInputExecPin>);
            OutputExecPins = new FilteredObservableCollection<NodeOutputExecPin, INodePin>(Pins, isType<NodeOutputExecPin>);
            InputTypePins = new FilteredObservableCollection<NodeInputTypePin, INodePin>(Pins, isType<NodeInputTypePin>);
            OutputTypePins = new FilteredObservableCollection<NodeOutputTypePin, INodePin>(Pins, isType<NodeOutputTypePin>);
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
        protected void AddInputDataPin(string pinName, ObservableValue<BaseType> pinType)
        {
            Pins.Add(new NodeInputDataPin(this, pinName, pinType));
        }

        /// <summary>
        /// Adds an output data pin to this node.
        /// </summary>
        /// <param name="pinName">Name of the pin.</param>
        /// <param name="pinType">Specifier for the type of this pin.</param>
        protected void AddOutputDataPin(string pinName, ObservableValue<BaseType> pinType)
        {
            Pins.Add(new NodeOutputDataPin(this, pinName, pinType));
        }

        /// <summary>
        /// Adds an input execution pin to this node.
        /// </summary>
        /// <param name="pinName">Name of the pin.</param>
        protected void AddInputExecPin(string pinName)
        {
            Pins.Add(new NodeInputExecPin(this, pinName));
        }

        /// <summary>
        /// Adds an output execution pin to this node.
        /// </summary>
        /// <param name="pinName">Name of the pin.</param>
        protected void AddOutputExecPin(string pinName)
        {
            Pins.Add(new NodeOutputExecPin(this, pinName));
        }

        /// <summary>
        /// Adds an input data pin to this node.
        /// </summary>
        /// <param name="pinName">Name of the pin.</param>
        protected void AddInputTypePin(string pinName)
        {
            var typePin = new NodeInputTypePin(this, pinName);
            typePin.IncomingPinChanged += OnIncomingTypePinChanged;
            Pins.Add(typePin);
        }

        /// <summary>
        /// Adds an output data pin to this node.
        /// </summary>
        /// <param name="pinName">Name of the pin.</param>
        /// <param name="getOutputTypeFunc">Function that generates the output type.</param>
        protected void AddOutputTypePin(string pinName, ObservableValue<BaseType> outputType)
        {
            Pins.Add(new NodeOutputTypePin(this, pinName, outputType));
        }

        private void OnIncomingTypePinChanged(NodeInputTypePin pin, NodeOutputTypePin oldPin, NodeOutputTypePin newPin)
        {
            if (oldPin?.InferredType != null)
                oldPin.InferredType.OnValueChanged -= EventInputTypeChanged;

            if (newPin?.InferredType != null)
                newPin.InferredType.OnValueChanged += EventInputTypeChanged;

            EventInputTypeChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when anything about the input type arguments changes.
        /// </summary>
        public event EventHandler InputTypeChanged;

        private void EventInputTypeChanged(object sender, EventArgs eventArgs)
        {
            OnInputTypeChanged(sender, eventArgs);

            // Notify others afterwards, since the above call might have updated something
            InputTypeChanged?.Invoke(sender, eventArgs);
        }

        protected virtual void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
        }

        [OnDeserialized]
        private void OnDeserializing(StreamingContext context)
        {
            SetupPinViews();

            foreach (var inputTypePin in InputTypePins)
            {
                if (inputTypePin.InferredType != null)
                    inputTypePin.InferredType.OnValueChanged += EventInputTypeChanged;
                inputTypePin.IncomingPinChanged += OnIncomingTypePinChanged;
            }
        }

        /// <summary>
        /// Called when the containing method was deserialized.
        /// </summary>
        public virtual void OnMethodDeserialized()
        {
            // Call OnInputTypeChanged to update the types of all nodes correctly.
            OnInputTypeChanged(this, null);
        }
    }
}
