using NetPrintsEditor.ViewModels;
using System.Collections.Generic;

namespace NetPrintsEditor.Messages
{
    /// <summary>
    /// Message for selecting and deselecting nodes.
    /// </summary>
    public class NodeSelectionMessage
    {
        /// <summary>
        /// Nodes to be selected.
        /// </summary>
        public IEnumerable<NodeVM> Nodes { get; }

        /// <summary>
        /// Whether to deselect any previous nodes.
        /// </summary>
        public bool DeselectPrevious { get; }

        /// <summary>
        /// Creates a node selection message to deselect all previous
        /// nodes and select the given node.
        /// </summary>
        /// <param name="node">Node to select.</param>
        public NodeSelectionMessage(NodeVM node)
        {
            Nodes = new[] { node };
            DeselectPrevious = true;
        }

        /// <summary>
        /// Creates a node selection message to optionally deselect
        /// all previous nodes and select the given nodes.
        /// </summary>
        /// <param name="nodes">Nodes to select.</param>
        /// <param name="deselectPrevious">Whether to deselect all previous nodes.</param>
        public NodeSelectionMessage(IEnumerable<NodeVM> nodes, bool deselectPrevious)
        {
            Nodes = nodes;
            DeselectPrevious = deselectPrevious;
        }

        /// <summary>
        /// Message to deselect all nodes.
        /// </summary>
        public static NodeSelectionMessage DeselectAll => new NodeSelectionMessage(new NodeVM[0], true);
    }
}
