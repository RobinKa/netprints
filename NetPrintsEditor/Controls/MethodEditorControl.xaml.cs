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

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for FunctionEditorControl.xaml
    /// </summary>
    public partial class MethodEditorControl : UserControl
    {
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

        public static DependencyProperty MethodProperty = DependencyProperty.Register("Method", typeof(Method), typeof(MethodEditorControl));

        private List<NodeControl> nodeControls = new List<NodeControl>();

        public MethodEditorControl()
        {
            InitializeComponent();
        }

        private void CreateNodeControl(Node node)
        {
            NodeControl nodeControl = new NodeControl(new NodeVM(node));
            nodeControl.Width = 150;
            nodeControl.Height = 100;

            nodeControls.Add(nodeControl);

            canvas.Children.Add(nodeControl);

            DragAdorner dragAdorner = new DragAdorner(nodeControl);

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
    }
}
