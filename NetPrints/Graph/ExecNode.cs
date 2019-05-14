using NetPrints.Core;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Abstract class for nodes that can be executed.
    /// </summary>
    [DataContract]
    [KnownType(typeof(CallMethodNode))]
    [KnownType(typeof(ConstructorNode))]
    public abstract class ExecNode : Node
    {
        protected ExecNode(NodeGraph graph)
            : base(graph)
        {
            AddExecPins();
        }

        private void AddExecPins()
        {
            AddInputExecPin("Exec");
            AddOutputExecPin("Exec");
        }

        protected override void SetPurity(bool pure)
        {
            base.SetPurity(pure);

            if (pure)
            {
                GraphUtil.DisconnectPin(InputExecPins[0]);
                Pins.Remove(Pins.First(pin => pin is NodeInputExecPin));
                
                GraphUtil.DisconnectPin(OutputExecPins[0]);
                Pins.Remove(Pins.First(pin => pin is NodeOutputExecPin));
            }
            else if (!pure)
            {
                AddExecPins();
            }
        }
    }
}
