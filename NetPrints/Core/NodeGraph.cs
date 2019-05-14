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

        public ObservableRangeCollection<PinConnection> Connections { get; private set; } = new ObservableRangeCollection<PinConnection>();

        public NodeGraph()
        {
            Nodes.CollectionChanged += OnNodeCollectionChanged;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            SetupInitialConnections();
        }

        private void SetupInitialConnections()
        {
            if (Connections is null)
            {
                Connections = new ObservableRangeCollection<PinConnection>();
            }

            OnNodeCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Nodes, 0));
            Nodes.CollectionChanged += OnNodeCollectionChanged;
        }

        private void OnNodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var node in e.OldItems.Cast<INode>())
                {
                    SetupNodeEvents(node, false);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var node in e.NewItems.Cast<INode>())
                {
                    SetupNodeEvents(node, true);
                }
            }
        }

        private void OnNodePinsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var pin in e.OldItems.Cast<NodePin>())
                {
                    SetupPinEvents(pin, false);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var pin in e.NewItems.Cast<NodePin>())
                {
                    SetupPinEvents(pin, true);
                }
            }
        }

        private void OnPinConnectionsCollectionChanged(NodePin fromPin, object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                var removedConnections = Connections.Where(conn =>
                        (conn.PinA == fromPin && e.OldItems.Contains(conn.PinB))
                        || (conn.PinB == fromPin && e.OldItems.Contains(conn.PinA)))
                    .ToArray();

                Connections.RemoveRange(removedConnections);
            }

            if (e.NewItems != null)
            {
                Connections.AddRange(e.NewItems.Cast<NodePin>()
                    .Where(pin => pin.ConnectionType == NodePinConnectionType.Single)
                    .Select(toPin => new PinConnection(fromPin, toPin)));
            }
        }

        private void SetupNodeEvents(INode node, bool add)
        {
            if (!add)
            {
                node.Pins.CollectionChanged -= OnNodePinsCollectionChanged;
            }

            foreach (var pin in node.Pins.Cast<NodePin>())
            {
                SetupPinEvents(pin, add);
            }

            if (add)
            {
                node.Pins.CollectionChanged += OnNodePinsCollectionChanged;
            }
        }

        private void SetupPinEvents(NodePin pin, bool add)
        {
            if (add)
            {
                if (pin.ConnectionType == NodePinConnectionType.Single)
                {
                    Connections.AddRange(pin.ConnectedPins.Select(toPin => new PinConnection((NodePin)pin, (NodePin)toPin)));
                }

                pin.ConnectedPins.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => OnPinConnectionsCollectionChanged(pin, sender, e);
            }
            else
            {
                //pin.ConnectedPins.CollectionChanged -= (object sender, NotifyCollectionChangedEventArgs e) => OnPinConnectionsCollectionChanged(pin, sender, e);
                Connections.RemoveRange(Connections.Where(conn => conn.PinA == pin || conn.PinB == pin).ToArray());
            }
        }
    }
}
