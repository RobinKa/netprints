using NetPrints.Core;
using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NetPrintsEditor.ViewModels
{
    public class MethodVM : INotifyPropertyChanged
    {
        public NodeVM SelectedNode
        {
            get => selectedNode;
            set
            {
                if (selectedNode != value)
                {
                    if (selectedNode != null)
                    {
                        selectedNode.IsSelected = false;
                    }

                    selectedNode = value;

                    if (selectedNode != null)
                    {
                        selectedNode.IsSelected = true;
                    }

                    OnPropertyChanged();
                }
            }
        }

        private NodeVM selectedNode;

        public string Name
        {
            get => method.Name;
            set
            {
                if(method.Name != value)
                {
                    method.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableViewModelCollection<NodeVM, Node> Nodes
        {
            get => nodes;
            set
            {
                if(nodes != value)
                {
                    nodes = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableViewModelCollection<NodeVM, Node> nodes;

        public ObservableRangeCollection<Type> ArgumentTypes
        {
            get => method.ArgumentTypes;
        }

        public ClassVM Class
        {
            get => classVM;
            set
            {
                if (classVM != value)
                {
                    classVM = value;
                    OnPropertyChanged();
                }
            }
        }

        private ClassVM classVM;

        public ObservableRangeCollection<Type> ReturnTypes
        {
            get => method.ReturnTypes;
        }

        public MethodModifiers Modifiers
        {
            get => method.Modifiers;
            set
            {
                if (method.Modifiers != value)
                {
                    method.Modifiers = value;
                    OnPropertyChanged();
                }
            }
        }

        public Method Method
        {
            get => method;
            set
            {
                if (method != value)
                {
                    if (method != null)
                    {
                        // Unbind nodes changed event
                        // Unbind all nodes' pins changed events

                        Nodes.CollectionChanged -= OnNodeCollectionChanged;
                        foreach (NodeVM node in Nodes)
                        {
                            node.InputDataPins.CollectionChanged -= OnPinCollectionChanged;
                            node.OutputDataPins.CollectionChanged -= OnPinCollectionChanged;
                            node.InputExecPins.CollectionChanged -= OnPinCollectionChanged;
                            node.OutputExecPins.CollectionChanged -= OnPinCollectionChanged;
                        }
                    }

                    method = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AllPins));

                    if (method != null)
                    {
                        // Bind nodes changed event
                        // Bind all nodes' pins changed events

                        Nodes.CollectionChanged += OnNodeCollectionChanged;
                        foreach (NodeVM node in Nodes)
                        {
                            node.InputDataPins.CollectionChanged += OnPinCollectionChanged;
                            node.OutputDataPins.CollectionChanged += OnPinCollectionChanged;
                            node.InputExecPins.CollectionChanged += OnPinCollectionChanged;
                            node.OutputExecPins.CollectionChanged += OnPinCollectionChanged;
                        }
                    }
                }
            }
        }

        private void OnNodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Unbind old node pins
            // Bind new node pins

            if (e.OldItems != null)
            {
                foreach (NodeVM node in e.OldItems.Cast<NodeVM>())
                {
                    node.InputDataPins.CollectionChanged -= OnPinCollectionChanged;
                    node.OutputDataPins.CollectionChanged -= OnPinCollectionChanged;
                    node.InputExecPins.CollectionChanged -= OnPinCollectionChanged;
                    node.OutputExecPins.CollectionChanged -= OnPinCollectionChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (NodeVM node in e.NewItems.Cast<NodeVM>())
                {
                    node.InputDataPins.CollectionChanged += OnPinCollectionChanged;
                    node.OutputDataPins.CollectionChanged += OnPinCollectionChanged;
                    node.InputExecPins.CollectionChanged += OnPinCollectionChanged;
                    node.OutputExecPins.CollectionChanged += OnPinCollectionChanged;
                }
            }

            OnPropertyChanged(nameof(AllPins));
        }

        private void OnPinCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(AllPins));
        }

        public IEnumerable<NodePinVM> AllPins
        {
            get
            {
                List<NodePinVM> pins = new List<NodePinVM>();

                if (Method != null)
                {
                    foreach (NodeVM node in Nodes)
                    {
                        pins.AddRange(node.InputDataPins);
                        pins.AddRange(node.OutputDataPins);
                        pins.AddRange(node.InputExecPins);
                        pins.AddRange(node.OutputExecPins);
                    }
                }

                return pins;
            }
        }

        private Method method;

        public MethodVM(Method method)
        {
            Method = method;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(Method))
            {
                Nodes = new ObservableViewModelCollection<NodeVM, Node>(Method.Nodes, n => new NodeVM(n));

                // Setup connections from new PinVMs to PinVMs
                foreach (NodeVM node in Nodes)
                {
                    foreach (NodePinVM pinVM in node.OutputExecPins)
                    {
                        NodeOutputExecPin pin = pinVM.Pin as NodeOutputExecPin;

                        if (pin.OutgoingPin != null)
                        {
                            NodeInputExecPin connPin = pin.OutgoingPin as NodeInputExecPin;
                            pinVM.ConnectedPin = Nodes.Where(n => n.Node == connPin.Node).Single().
                                InputExecPins.Single(x => x.Pin == connPin);
                        }
                    }

                    foreach (NodePinVM pinVM in node.InputDataPins)
                    {
                        NodeInputDataPin pin = pinVM.Pin as NodeInputDataPin;

                        if (pin.IncomingPin != null)
                        {
                            NodeOutputDataPin connPin = pin.IncomingPin as NodeOutputDataPin;
                            pinVM.ConnectedPin = Nodes.Where(n => n.Node == connPin.Node).Single().
                                OutputDataPins.Single(x => x.Pin == connPin);
                        }
                    }
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
