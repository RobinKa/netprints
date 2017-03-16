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
            get
            {
                return GetValue(MethodProperty) as Method;
            }
            set
            {
                SetValue(MethodProperty, value);
                
                // Remove previous node controls
                foreach (NodeControl control in nodeControls)
                {
                    canvas.Children.Remove(control);
                }

                nodeControls.Clear();

                // Create node controls for method nodes
                foreach(Node node in value.Nodes)
                {
                    CreateNodeControl(node);
                }
            }
        }

        public List<NodeControl> NodeControls
        {
            get => nodeControls;
        }

        public static DependencyProperty MethodProperty = DependencyProperty.Register(
            nameof(Method), typeof(Method), typeof(MethodEditorControl));

        private List<NodeControl> nodeControls = new List<NodeControl>();

        public static DependencyProperty SuggestedFunctionsProperty = DependencyProperty.Register(
            nameof(SuggestedFunctions), typeof(ObservableCollection<MethodInfo>), typeof(MethodEditorControl));

        public ObservableCollection<MethodInfo> SuggestedFunctions
        {
            get => (ObservableCollection<MethodInfo>)GetValue(SuggestedFunctionsProperty);
            set => SetValue(SuggestedFunctionsProperty, value);
        }

        public MethodEditorControl()
        {
            SuggestedFunctions = new ObservableCollection<MethodInfo>(ReflectionUtil.GetStaticFunctions());
            InitializeComponent();
        }

        public void CreateNodeControl(Node node)
        {
            grid.ContextMenu.IsOpen = false;

            NodeControl nodeControl = new NodeControl(new NodeVM(node));
            
            nodeControls.Add(nodeControl);

            canvas.Children.Add(nodeControl);

            DragAdorner dragAdorner = new DragAdorner(nodeControl, GridCellSize);

            // Make set node position command when dragging is done
            dragAdorner.OnDragEnd += (sender, e) =>
            {
                if (nodeControl.RenderTransform is TranslateTransform t)
                {
                    UndoRedoStack.Instance.DoCommand(NetPrintsCommands.SetNodePosition,
                        new NetPrintsCommands.SetNodePositionParameters(nodeControl.NodeVM, t.X, t.Y));
                }
            };

            AdornerLayer.GetAdornerLayer(nodeControl)?.Add(dragAdorner);
        }

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
            }
            else if(e.Data.GetDataPresent(typeof(PinControl)))
            {
                PinControl pinControl = e.Data.GetData(typeof(PinControl)) as PinControl;

                if (pinControl.Pin is NodeOutputDataPin odp)
                {
                    MethodInfo[] methods = odp.PinType.GetMethods();
                    // TODO: Set context menu list to methods
                }
            }
            if (Method != null && e.Data.GetDataPresent(typeof(Method)))
            {
                Point mousePosition = e.GetPosition(canvas);
                Method method = e.Data.GetData(typeof(Method)) as Method;

                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(CallMethodNode), Method, mousePosition.X, mousePosition.Y,
                    method.Name, method.ArgumentTypes, method.ReturnTypes
                ));
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
    }
}
