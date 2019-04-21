using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetPrintsEditor.Messages
{
    /// <summary>
    /// Message for opening a graph.
    /// </summary>
    public class OpenGraphMessage
    {
        /// <summary>
        /// Graph to open.
        /// </summary>
        public NodeGraph Graph { get; }

        public OpenGraphMessage(NodeGraph graph)
        {
            Graph = graph;
        }
    }
}
