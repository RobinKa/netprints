using NetPrints.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public abstract class Node
    {
        public ObservableCollection<NodeInputDataPin> InputDataPins { get; } = new ObservableCollection<NodeInputDataPin>();
        public ObservableCollection<NodeOutputDataPin> OutputDataPins { get; } = new ObservableCollection<NodeOutputDataPin>();

        public ObservableCollection<NodeInputExecPin> InputExecPins { get; } = new ObservableCollection<NodeInputExecPin>();
        public ObservableCollection<NodeOutputExecPin> OutputExecPins { get; } = new ObservableCollection<NodeOutputExecPin>();

        public double PositionX { get; set; }
        public double PositionY { get; set; }

        public string Name { get; set; }

        public bool IsPure
        {
            get
            {
                return InputExecPins.Count == 0 && OutputExecPins.Count == 0;
            }
        }

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
