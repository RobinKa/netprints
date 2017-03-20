using NetPrints.Core;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    [KnownType(typeof(CallMethodNode))]
    [KnownType(typeof(CallStaticFunctionNode))]
    [KnownType(typeof(EntryNode))]
    [KnownType(typeof(ForLoopNode))]
    [KnownType(typeof(IfElseNode))]
    [KnownType(typeof(LiteralNode))]
    [KnownType(typeof(ReturnNode))]
    [KnownType(typeof(VariableGetterNode))]
    [KnownType(typeof(VariableSetterNode))]
    [KnownType(typeof(ConstructorNode))]
    public abstract class Node
    {
        [DataMember]
        public ObservableRangeCollection<NodeInputDataPin> InputDataPins { get; private set; } = new ObservableRangeCollection<NodeInputDataPin>();

        [DataMember]
        public ObservableRangeCollection<NodeOutputDataPin> OutputDataPins { get; private set; } = new ObservableRangeCollection<NodeOutputDataPin>();

        [DataMember]
        public ObservableRangeCollection<NodeInputExecPin> InputExecPins { get; private set; } = new ObservableRangeCollection<NodeInputExecPin>();

        [DataMember]
        public ObservableRangeCollection<NodeOutputExecPin> OutputExecPins { get; private set; } = new ObservableRangeCollection<NodeOutputExecPin>();

        public delegate void NodePositionChangedDelegate(Node node, double positionX, double positionY);
        public event NodePositionChangedDelegate OnPositionChanged;

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

        [DataMember]
        public string Name { get; set; }

        public bool IsPure
        {
            get
            {
                return InputExecPins.Count == 0 && OutputExecPins.Count == 0;
            }
        }

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

        protected void AddInputDataPin(string pinName, Type pinType)
        {
            InputDataPins.Add(new NodeInputDataPin(this, pinName, pinType));
        }

        protected void AddOutputDataPin(string pinName, Type pinType)
        {
            OutputDataPins.Add(new NodeOutputDataPin(this, pinName, pinType));;
        }

        protected void AddInputExecPin(string pinName)
        {
            InputExecPins.Add(new NodeInputExecPin(this, pinName));
        }

        protected void AddOutputExecPin(string pinName)
        {
            OutputExecPins.Add(new NodeOutputExecPin(this, pinName));
        }
    }
}
