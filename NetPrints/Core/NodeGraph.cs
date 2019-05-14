using NetPrints.Base;
using NetPrints.Graph;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    [DataContract]
    [KnownType(typeof(MethodGraph))]
    [KnownType(typeof(ConstructorGraph))]
    [KnownType(typeof(ClassGraph))]
    [KnownType(typeof(TypeGraph))]
    public abstract class NodeGraph : INodeGraph
    {
        /// <summary>
        /// Collection of nodes in this graph.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<INode> Nodes
        {
            get;
            private set;
        } = new ObservableRangeCollection<INode>();

        /// <summary>
        /// Class this graph is contained in.
        /// </summary>
        [DataMember]
        public ClassGraph Class
        {
            get;
            set;
        }

        /// <summary>
        /// Project the graph is part of.
        /// </summary>
        public Project Project
        {
            get;
            set;
        }
    }
}
