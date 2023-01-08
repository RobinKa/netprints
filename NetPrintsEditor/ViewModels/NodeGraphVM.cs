using GalaSoft.MvvmLight;
using NetPrints.Base;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Controls;
using NetPrintsEditor.Dialogs;
using NetPrintsEditor.Messages;
using NetPrintsEditor.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NetPrintsEditor.ViewModels
{
    public class NodeGraphVM : ViewModelBase
    {
        public IEnumerable<SearchableComboBoxItem> Suggestions
        {
            get => SuggestionViewModel.Items;
            private set => SuggestionViewModel.Items = value;
        }

        /// <summary>
        /// Pin that was dragged to generate suggestions.
        /// Null if that suggestions were not created for a pin.
        /// </summary>
        public NodePin SuggestionPin { get; set; }

        public ObservableRangeCollection<PinConnection> Connections => connectionTracker.Connections;

        private GraphConnectionTracker connectionTracker;

        public SuggestionListVM SuggestionViewModel { get; } = new SuggestionListVM();

        public event EventHandler OnHideContextMenu;

        private readonly ISuggestionGenerator suggestionGenerator = new SuggestionGenerator();

        public void UpdateSuggestions(double mouseX, double mouseY)
        {
            SuggestionViewModel.Graph = Graph;
            SuggestionViewModel.PositionX = mouseX;
            SuggestionViewModel.PositionY = mouseY;
            SuggestionViewModel.SuggestionPin = SuggestionPin;
            SuggestionViewModel.HideContextMenu = () => OnHideContextMenu?.Invoke(this, EventArgs.Empty);

            Suggestions = suggestionGenerator.GetSuggestions(Graph, SuggestionPin).Select(x => new SearchableComboBoxItem(x.Item1, x.Item2));
        }

        public IEnumerable<NodeVM> SelectedNodes
        {
            get => selectedNodes;
            private set
            {
                if (selectedNodes != value)
                {
                    // Deselect old nodes
                    if (selectedNodes != null)
                    {
                        foreach (var node in selectedNodes)
                        {
                            node.IsSelected = false;
                        }
                    }

                    selectedNodes = value;

                    // Select new nodes
                    if (selectedNodes != null)
                    {
                        foreach (var node in selectedNodes)
                        {
                            node.IsSelected = true;
                        }
                    }
                }
            }
        }

        private IEnumerable<NodeVM> selectedNodes = new NodeVM[0];

        public string Name
        {
            get => graph is MethodGraph methodGraph ? methodGraph.Name : graph.ToString();
            set
            {
                if (graph is MethodGraph methodGraph && methodGraph.Name != value)
                {
                    methodGraph.Name = value;
                }
            }
        }

        public bool IsConstructor => graph is ConstructorGraph;

        public ObservableViewModelCollection<NodeVM, INode> Nodes { get; set; }

        public IEnumerable<BaseType> ArgumentTypes =>
            graph is ExecutionGraph execGraph ? execGraph.ArgumentTypes : null;

        public IEnumerable<Named<BaseType>> NamedArgumentTypes =>
            graph is ExecutionGraph execGraph ? execGraph.NamedArgumentTypes : null;

        public IEnumerable<BaseType> ReturnTypes =>
            graph is MethodGraph methodGraph ? methodGraph.ReturnTypes : null;

        public ClassEditorVM Class { get; set; }

        public MethodModifiers Modifiers
        {
            get => graph is MethodGraph methodGraph ? methodGraph.Modifiers : MethodModifiers.None;
            set
            {
                if (graph is MethodGraph methodGraph && methodGraph.Modifiers != value)
                {
                    methodGraph.Modifiers = value;
                }
            }
        }

        public MemberVisibility Visibility
        {
            get => graph is ExecutionGraph execGraph ? execGraph.Visibility : MemberVisibility.Invalid;
            set
            {
                if (graph is ExecutionGraph execGraph && execGraph.Visibility != value)
                {
                    execGraph.Visibility = value;
                }
            }
        }

        public IEnumerable<MemberVisibility> PossibleVisibilities => new[]
        {
            MemberVisibility.Internal,
            MemberVisibility.Private,
            MemberVisibility.Protected,
            MemberVisibility.Public,
        };

        #region Node dragging
        public double NodeDragScale
        {
            get;
            set;
        } = 1;

        private double nodeDragAccumX;
        private double nodeDragAccumY;

        private readonly Dictionary<NodeVM, (double X, double Y)> nodeStartPositions = new Dictionary<NodeVM, (double, double)>();

        private void SetupNodeDragEvents(NodeVM node, bool add)
        {
            if (add)
            {
                node.OnDragStart += OnNodeDragStart;
                node.OnDragEnd += OnNodeDragEnd;
                node.OnDragMove += OnNodeDragMove;
            }
            else
            {
                node.OnDragStart -= OnNodeDragStart;
                node.OnDragEnd -= OnNodeDragEnd;
                node.OnDragMove -= OnNodeDragMove;
            }
        }

        /// <summary>
        /// Called when a node starts dragging.
        /// </summary>
        private void OnNodeDragStart(NodeVM node)
        {
            nodeDragAccumX = 0;
            nodeDragAccumY = 0;
            nodeStartPositions.Clear();

            // Remember the initial positions of the selected nodes
            if (SelectedNodes != null)
            {
                foreach (var selectedNode in SelectedNodes)
                {
                    nodeStartPositions.Add(selectedNode, (selectedNode.Node.PositionX, selectedNode.Node.PositionY));
                }
            }
        }

        /// <summary>
        /// Called when a node ends dragging.
        /// </summary>
        private void OnNodeDragEnd(NodeVM node)
        {
            // Snap final position to grid
            if (SelectedNodes != null)
            {
                foreach (var selectedNode in SelectedNodes)
                {
                    selectedNode.Node.PositionX -= selectedNode.Node.PositionX % GraphEditorView.GridCellSize;
                    selectedNode.Node.PositionY -= selectedNode.Node.PositionY % GraphEditorView.GridCellSize;
                }
            }
        }

        /// <summary>
        /// Called when a node is dragging.
        /// </summary>
        private void OnNodeDragMove(NodeVM node, double dx, double dy)
        {
            nodeDragAccumX += dx * NodeDragScale;
            nodeDragAccumY += dy * NodeDragScale;

            // Move all selected nodes
            if (SelectedNodes != null)
            {
                foreach (var selectedNode in SelectedNodes)
                {
                    // Set position by taking total delta and adding it to the initial position
                    selectedNode.Node.PositionX = nodeStartPositions[selectedNode].X + nodeDragAccumX - nodeDragAccumX % GraphEditorView.GridCellSize;
                    selectedNode.Node.PositionY = nodeStartPositions[selectedNode].Y + nodeDragAccumY - nodeDragAccumY % GraphEditorView.GridCellSize;
                }
            }
        }
        #endregion

        public NodeGraph Graph
        {
            get => graph;
            set
            {
                if (graph != value)
                {
                    if (graph != null)
                    {
                        Nodes.CollectionChanged -= OnNodeCollectionChanged;
                        Nodes.ToList().ForEach(n => SetupNodeDragEvents(n, false));
                    }

                    graph = value;
                    RaisePropertyChanged(nameof(AllPins));
                    RaisePropertyChanged(nameof(Visibility));

                    Nodes = new ObservableViewModelCollection<NodeVM, INode>(Graph.Nodes, n => new NodeVM((Node)n));

                    if (graph != null)
                    {
                        Nodes.CollectionChanged += OnNodeCollectionChanged;
                        Nodes.ToList().ForEach(n => SetupNodeDragEvents(n, true));
                    }

                    connectionTracker = new GraphConnectionTracker(graph);
                    RaisePropertyChanged(nameof(Connections));
                }
            }
        }

        private NodeGraph graph;

        private void OnNodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                var removedNodes = e.OldItems.Cast<NodeVM>();
                removedNodes.ToList().ForEach(n => SetupNodeDragEvents(n, false));
            }

            if (e.NewItems != null)
            {
                var addedNodes = e.NewItems.Cast<NodeVM>();
                addedNodes.ToList().ForEach(n => SetupNodeDragEvents(n, true));
            }

            RaisePropertyChanged(nameof(AllPins));
        }

        public IEnumerable<NodePinVM> AllPins => Graph != null ? Nodes.SelectMany(node => node.Pins) : new NodePinVM[0];

        /// <summary>
        /// Sends a message to deselect all nodes and select the given nodes.
        /// </summary>
        /// <param name="nodes">Nodes to be selected.</param>
        public void SelectNodes(IEnumerable<NodeVM> nodes)
        {
            MessengerInstance.Send(new NodeSelectionMessage(nodes, true));
        }

        /// <summary>
        /// Sends a message to deselect all nodes.
        /// </summary>
        public void DeselectNodes()
        {
            MessengerInstance.Send(NodeSelectionMessage.DeselectAll);
        }

        public NodeGraphVM(NodeGraph graph)
        {
            Graph = graph;

            MessengerInstance.Register<NodeSelectionMessage>(this, OnNodeSelectionReceived);
            MessengerInstance.Register<AddNodeMessage>(this, OnAddNodeReceived);
        }

        private void OnNodeSelectionReceived(NodeSelectionMessage msg)
        {
            if (msg.DeselectPrevious)
            {
                SelectedNodes = new NodeVM[0] { };
            }

            SelectedNodes = SelectedNodes.Concat(msg.Nodes).Distinct();
        }

        private void OnAddNodeReceived(AddNodeMessage msg)
        {
            if (!msg.Handled && msg.Graph == Graph)
            {
                msg.Handled = true;

                // Make sure the node will be on the canvas
                if (msg.PositionX < 0)
                    msg.PositionX = 0;

                if (msg.PositionY < 0)
                    msg.PositionY = 0;

                object[] parameters = new object[] { msg.Graph }.Concat(msg.ConstructorParameters).ToArray();
                Node node = Activator.CreateInstance(msg.NodeType, parameters) as Node;
                node.PositionX = msg.PositionX;
                node.PositionY = msg.PositionY;

                // If the node was created as part of a suggestion, connect it
                // to the relevant node pin.
                if (msg.SuggestionPin != null)
                {
                    GraphUtil.ConnectRelevantPins(msg.SuggestionPin,
                        node, App.ReflectionProvider.TypeSpecifierIsSubclassOf,
                        App.ReflectionProvider.HasImplicitCast);
                }
            }
        }

        public void AddNode(AddNodeMessage msg) => OnAddNodeReceived(msg);
    }
}
