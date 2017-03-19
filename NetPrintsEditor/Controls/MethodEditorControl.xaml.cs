using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Translator;
using NetPrintsEditor.Adorners;
using NetPrintsEditor.ViewModels;
using NetPrintsEditor.Commands;
using System.Collections.ObjectModel;
using System.Reflection;
using NetPrints.Extensions;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for FunctionEditorControl.xaml
    /// </summary>
    public partial class MethodEditorControl : UserControl
    {
        public const double GridCellSize = 20;

        public Method Method
        {
            get => GetValue(MethodProperty) as Method;
            set => SetValue(MethodProperty, value);
        }

        public static DependencyProperty MethodProperty = DependencyProperty.Register(
            nameof(Method), typeof(Method), typeof(MethodEditorControl));
        
        public static DependencyProperty SuggestedFunctionsProperty = DependencyProperty.Register(
            nameof(SuggestedFunctions), typeof(ObservableCollection<MethodInfo>), typeof(MethodEditorControl));

        public ObservableCollection<MethodInfo> SuggestedFunctions
        {
            get => (ObservableCollection<MethodInfo>)GetValue(SuggestedFunctionsProperty);
            set => SetValue(SuggestedFunctionsProperty, value);
        }

        public MethodEditorControl()
        {
            InitializeComponent();
        }

        #region Hack: Initialize Visual Node Connections
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if(e.Property == MethodProperty)
            {
                InitializeVisualNodeConnections();
            }
        }

        private NodeControl FindNodeControl(Node node)
        {
            // NodeList -> ... -> Canvas -> ... -> NodeControl

            var nodeCanvas = 
                VisualTreeHelper.GetChild(
                VisualTreeHelper.GetChild(
                VisualTreeHelper.GetChild(nodeList,
                0), 0), 0);

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(nodeCanvas); i++)
            {
                var v = 
                    VisualTreeHelper.GetChild(
                    VisualTreeHelper.GetChild(nodeCanvas, i), 
                    0);

                if (v is NodeControl nc && nc.Node.Node == node)
                {
                    return nc;
                }
            }

            return null;
        }

        private void InitializeVisualNodeConnections()
        {
            if (Method != null)
            {
                // Wait so the nodes get created
                Task.Delay(100).ContinueWith(_ => Dispatcher.Invoke(() => 
                {
                    foreach (Node node in Method.Nodes)
                    {
                        NodeControl nodeControl = FindNodeControl(node);

                        // Visually connect pins
                        foreach (NodeInputDataPin pin in node.InputDataPins)
                        {
                            if (pin.IncomingPin != null)
                            {
                                NodeControl otherNodeControl = FindNodeControl(pin.IncomingPin.Node);

                                nodeControl.FindPinControl(pin).ConnectedPin =
                                    otherNodeControl.FindPinControl(pin.IncomingPin);
                            }
                        }

                        foreach (NodeOutputExecPin pin in node.OutputExecPins)
                        {
                            if (pin.OutgoingPin != null)
                            {
                                NodeControl otherNodeControl = FindNodeControl(pin.OutgoingPin.Node);

                                nodeControl.FindPinControl(pin).ConnectedPin =
                                    otherNodeControl.FindPinControl(pin.OutgoingPin);
                            }
                        }
                    }
                }));
            }
        }
        #endregion

        public void ShowVariableGetSet(Variable variable, Point position)
        {
            variableGetSet.Visibility = Visibility.Visible;
            variableGetSet.Tag = new object[] { variable, position };

            variableSetButton.Tag = variableGetSet.Tag;
            variableGetButton.Tag = variableGetSet.Tag;
            
            Canvas.SetLeft(variableGetSet, position.X - variableGetSet.Width / 2);
            Canvas.SetTop(variableGetSet, position.Y - variableGetSet.Height / 2);
        }

        public void HideVariableGetSet()
        {
            variableGetSet.Visibility = Visibility.Hidden;
        }

        private void OnVariableSetClicked(object sender, RoutedEventArgs e)
        {
            if(sender is Control c && c.Tag is object[] o && o.Length == 2 && o[0] is Variable v && o[1] is Point pos)
            {
                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(VariableSetterNode), Method, pos.X, pos.Y,
                    v.Name, v.VariableType
                ));
            }

            HideVariableGetSet();
        }

        private void OnVariableGetClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Control c && c.Tag is object[] o && o.Length == 2 && o[0] is Variable v && o[1] is Point pos)
            {
                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(VariableGetterNode), Method, pos.X, pos.Y,
                    v.Name, v.VariableType
                ));
            }

            HideVariableGetSet();
        }

        private void OnVariableGetSetMouseLeave(object sender, MouseEventArgs e)
        {
            HideVariableGetSet();
        }

        private void OnGridDrop(object sender, DragEventArgs e)
        {
            if (Method != null && e.Data.GetDataPresent(typeof(Variable)))
            {
                Variable variable = e.Data.GetData(typeof(Variable)) as Variable;
                
                ShowVariableGetSet(variable, e.GetPosition(variableGetSetCanvas));

                e.Handled = true;
            }
            else if(e.Data.GetDataPresent(typeof(PinControl)))
            {
                // Show all relevant methods for the type of the pin if its a data pin

                PinControl pinControl = e.Data.GetData(typeof(PinControl)) as PinControl;

                if (pinControl.Pin is NodeDataPin dataPin)
                {
                    if (dataPin is NodeOutputDataPin odp)
                    {
                        SuggestedFunctions = new ObservableCollection<MethodInfo>(
                            ReflectionUtil.GetPublicMethodsForType(odp.PinType));
                    }
                    else if (dataPin is NodeInputDataPin idp)
                    {
                        SuggestedFunctions = new ObservableCollection<MethodInfo>(
                            ReflectionUtil.GetStaticFunctionsWithReturnType(idp.PinType));
                    }

                    // Open the context menu
                    grid.ContextMenu.PlacementTarget = grid;
                    grid.ContextMenu.IsOpen = true;

                    e.Handled = true;
                }
            }
            if (Method != null && e.Data.GetDataPresent(typeof(Method)))
            {
                Point mousePosition = e.GetPosition(methodEditorWindow);
                Method method = e.Data.GetData(typeof(Method)) as Method;

                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(CallMethodNode), Method, mousePosition.X, mousePosition.Y,
                    method.Name, method.ArgumentTypes, method.ReturnTypes
                ));

                e.Handled = true;
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (Method != null && e.Data.GetDataPresent(typeof(Variable)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else if(e.Data.GetDataPresent(typeof(PinControl)))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
            }
            else if(Method != null && e.Data.GetDataPresent(typeof(Method)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            SuggestedFunctions = new ObservableCollection<MethodInfo>(ReflectionUtil.GetStaticFunctions());
        }
    }
}
