using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Abstract class for execution pins.
    /// </summary>
    [DataContract]
    public abstract class NodeExecPin : NodePin
    {
        protected NodeExecPin(Node node, string name)
            : base(node, name)
        {
        }
    }
}
