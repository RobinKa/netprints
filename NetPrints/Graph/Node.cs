using NetPrints.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
    public abstract class Node
    {
        [DataMember]
        public ObservableCollection<NodeInputDataPin> InputDataPins { get; private set; } = new ObservableCollection<NodeInputDataPin>();

        [DataMember]
        public ObservableCollection<NodeOutputDataPin> OutputDataPins { get; private set; } = new ObservableCollection<NodeOutputDataPin>();

        [DataMember]
        public ObservableCollection<NodeInputExecPin> InputExecPins { get; private set; } = new ObservableCollection<NodeInputExecPin>();

        [DataMember]
        public ObservableCollection<NodeOutputExecPin> OutputExecPins { get; private set; } = new ObservableCollection<NodeOutputExecPin>();

        [DataMember]
        public double PositionX { get; set; }

        [DataMember]
        public double PositionY { get; set; }

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
            return GraphUtil.SplitCamelCase(Name);
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
