using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public abstract class Node
    {
        public IList<NodeInputDataPin> InputDataPins { get; } = new List<NodeInputDataPin>();
        public IList<NodeOutputDataPin> OutputDataPins { get; } = new List<NodeOutputDataPin>();

        public IList<NodeInputExecPin> InputExecPins { get; } = new List<NodeInputExecPin>();
        public IList<NodeOutputExecPin> OutputExecPins { get; } = new List<NodeOutputExecPin>();

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
