using NetPrints.Core;

namespace NetPrints.Base
{
    /// <summary>
    /// Interface for all node types.
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// All pins on this node.
        /// </summary>
        public ObservableRangeCollection<INodePin> Pins { get; }

        /// <summary>
        /// Name of this node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Graph this node is contained in.
        /// </summary>
        public INodeGraph Graph
        {
            get;
        }
    }
}
