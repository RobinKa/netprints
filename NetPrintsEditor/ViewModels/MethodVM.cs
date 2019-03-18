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

        public ObservableRangeCollection<BaseType> ArgumentTypes
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

        public ObservableRangeCollection<BaseType> ReturnTypes
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

        /*
        Scenarios:
        
        Method changed [0]:
            (Un)assign (old) nodes changed events [1]
            (Un)assign (old)new pins changed events [2]
            (Un)assign (old)new pin connection changed events [3]
            (Un)setup (old)new method pins' connections in VM

        Node changed [1]:
            (Un)assign (old)new pins changed events [2]
            (Un)assign (old)new pin connection changed events [3]
            (Un)setup (old)new node pins' connections in VM

        Pin changed [2]:
            (Un)assign (old)new pin connection changed events [3]
            (Un)setup (old)new pin connection in VM

        Pin connection changed [3]:
            (Un)setup (old)new connection in VM
        */

        private void SetupNodeEvents(NodeVM node, bool add)
        {
            // (Un)assign (old)new pins changed events [2]
            if (add)
            {
                node.InputDataPins.CollectionChanged += OnPinCollectionChanged;
                node.OutputDataPins.CollectionChanged += OnPinCollectionChanged;
                node.InputExecPins.CollectionChanged += OnPinCollectionChanged;
                node.OutputExecPins.CollectionChanged += OnPinCollectionChanged;
                node.InputTypePins.CollectionChanged += OnPinCollectionChanged;
                node.OutputTypePins.CollectionChanged += OnPinCollectionChanged;
            }
            else
            {
                node.InputDataPins.CollectionChanged -= OnPinCollectionChanged;
                node.OutputDataPins.CollectionChanged -= OnPinCollectionChanged;
                node.InputExecPins.CollectionChanged -= OnPinCollectionChanged;
                node.OutputExecPins.CollectionChanged -= OnPinCollectionChanged;
                node.InputTypePins.CollectionChanged -= OnPinCollectionChanged;
                node.OutputTypePins.CollectionChanged -= OnPinCollectionChanged;
            }

            // (Un)assign (old)new pin connection changed events [3]
            node.InputDataPins.ToList().ForEach(p => SetupPinEvents(p, add));
            node.OutputExecPins.ToList().ForEach(p => SetupPinEvents(p, add));
        }

        private void SetupPinEvents(NodePinVM pin, bool add)
        {
            // (Un)assign (old)new pin connection changed events [3]

            if (pin.Pin is NodeInputDataPin idp)
            {
                if (add)
                {
                    idp.IncomingPinChanged += OnInputDataPinIncomingPinChanged;
                }
                else
                {
                    idp.IncomingPinChanged -= OnInputDataPinIncomingPinChanged;
                }
            }
            else if (pin.Pin is NodeOutputExecPin oxp)
            {
                if (add)
                {
                    oxp.OutgoingPinChanged += OnOutputExecPinIncomingPinChanged;
                }
                else
                {
                    oxp.OutgoingPinChanged -= OnOutputExecPinIncomingPinChanged;
                }
            }
            else if (pin.Pin is NodeInputTypePin itp)
            {
                if (add)
                {
                    itp.IncomingPinChanged += OnInputTypePinIncomingPinChanged;
                }
                else
                {
                    itp.IncomingPinChanged -= OnInputTypePinIncomingPinChanged;
                }
            }
        }

        private void SetupPinConnection(NodePinVM pin, bool add)
        {
            if (pin.Pin is NodeInputDataPin idp)
            {
                if (idp.IncomingPin != null)
                {
                    if (add)
                    {
                        NodeOutputDataPin connPin = idp.IncomingPin as NodeOutputDataPin;
                        pin.ConnectedPin = Nodes
                            .Single(n => n.Node == connPin.Node)
                            .OutputDataPins
                            .Single(x => x.Pin == connPin);
                    }
                    else
                    {
                        pin.ConnectedPin = null;
                    }
                }
            }
            else if (pin.Pin is NodeOutputExecPin oxp)
            {
                if (oxp.OutgoingPin != null)
                {
                    if (add)
                    {
                        NodeInputExecPin connPin = oxp.OutgoingPin as NodeInputExecPin;
                        pin.ConnectedPin = Nodes
                            .Single(n => n.Node == connPin.Node)
                            .InputExecPins
                            .Single(x => x.Pin == connPin);
                    }
                    else
                    {
                        pin.ConnectedPin = null;
                    }
                }
            }
            else if (pin.Pin is NodeInputTypePin itp)
            {
                if (itp.IncomingPin != null)
                {
                    if (add)
                    {
                        NodeOutputTypePin connPin = itp.IncomingPin as NodeOutputTypePin;
                        pin.ConnectedPin = Nodes
                            .Single(n => n.Node == connPin.Node)
                            .OutputTypePins
                            .Single(x => x.Pin == connPin);
                    }
                    else
                    {
                        pin.ConnectedPin = null;
                    }
                }
            }
        }

        private void SetupNodeConnections(NodeVM node, bool add)
        {
            node.OutputExecPins.ToList().ForEach(p => SetupPinConnection(p, add));
            node.InputDataPins.ToList().ForEach(p => SetupPinConnection(p, add));
            node.InputTypePins.ToList().ForEach(p => SetupPinConnection(p, add));

            // Make sure to remove the IXP and ODP connections 
            // any other nodes are connecting to too

            if (!add)
            {
                AllPins.Where(p =>
                    node.InputExecPins.Contains(p.ConnectedPin) ||
                    node.OutputDataPins.Contains(p.ConnectedPin) ||
                    node.OutputTypePins.Contains(p.ConnectedPin)
                ).ToList().ForEach(p => p.ConnectedPin = null);
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
                        Nodes.CollectionChanged -= OnNodeCollectionChanged;
                        Nodes.ToList().ForEach(n => SetupNodeEvents(n, false));

                        Nodes.ToList().ForEach(n => SetupNodeConnections(n, false));
                    }

                    method = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AllPins));

                    Nodes = new ObservableViewModelCollection<NodeVM, Node>(Method.Nodes, n => new NodeVM(n));
                    
                    if (method != null)
                    {
                        Nodes.CollectionChanged += OnNodeCollectionChanged;
                        Nodes.ToList().ForEach(n => SetupNodeEvents(n, true));

                        // Connections on normal pins already exist,
                        // Set them up for the viewmodels of the pins
                        Nodes.ToList().ForEach(n => SetupNodeConnections(n, true));
                    }
                }
            }
        }

        private void OnNodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                var removedNodes = e.OldItems.Cast<NodeVM>();
                removedNodes.ToList().ForEach(n => SetupNodeEvents(n, false));

                removedNodes.ToList().ForEach(n => SetupNodeConnections(n, false));
            }

            if (e.NewItems != null)
            {
                var addedNodes = e.NewItems.Cast<NodeVM>();
                addedNodes.ToList().ForEach(n => SetupNodeEvents(n, true));

                addedNodes.ToList().ForEach(n => SetupNodeConnections(n, true));
            }

            OnPropertyChanged(nameof(AllPins));
        }

        private void OnPinCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                var removedPins = e.OldItems.Cast<NodePinVM>();

                // Unsetup initial connections of added pins
                removedPins.ToList().ForEach(p => SetupPinConnection(p, false));

                // Remove events for removed pins
                removedPins.ToList().ForEach(p => SetupPinEvents(p, false));
            }

            if(e.NewItems != null)
            {
                var addedPins = e.NewItems.Cast<NodePinVM>();

                // Setup initial connections of added pins
                addedPins.ToList().ForEach(p => SetupPinConnection(p, true));

                // Add events for added pins
                addedPins.ToList().ForEach(p => SetupPinEvents(p, true));
            }

            OnPropertyChanged(nameof(AllPins));
        }

        private void OnInputDataPinIncomingPinChanged(NodeInputDataPin pin, NodeOutputDataPin oldPin, NodeOutputDataPin newPin)
        {
            // Connect pinVM newPinVM (or null if newPin is null)

            NodePinVM pinVM = FindPinVMFromPin(pin);
            pinVM.ConnectedPin = newPin == null ? null : FindPinVMFromPin(newPin);
        }

        private void OnOutputExecPinIncomingPinChanged(NodeOutputExecPin pin, NodeInputExecPin oldPin, NodeInputExecPin newPin)
        {
            // Connect pinVM newPinVM (or null if newPin is null)

            NodePinVM pinVM = FindPinVMFromPin(pin);
            pinVM.ConnectedPin = newPin == null ? null : FindPinVMFromPin(newPin);
        }

        private void OnInputTypePinIncomingPinChanged(NodeInputTypePin pin, NodeOutputTypePin oldPin, NodeOutputTypePin newPin)
        {
            // Connect pinVM newPinVM (or null if newPin is null)

            NodePinVM pinVM = FindPinVMFromPin(pin);
            pinVM.ConnectedPin = newPin == null ? null : FindPinVMFromPin(newPin);
        }

        private NodePinVM FindPinVMFromPin(NodePin pin)
        {
            return AllPins.Single(p => p.Pin == pin);
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
                        pins.AddRange(node.InputTypePins);
                        pins.AddRange(node.OutputTypePins);
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
