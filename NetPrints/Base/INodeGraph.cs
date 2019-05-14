using NetPrints.Core;

namespace NetPrints.Base
{
    public interface INodeGraph
    {
        /// <summary>
        /// List of nodes in this graph.
        /// </summary>
        public ObservableRangeCollection<INode> Nodes
        {
            get;
        }
    }
}
