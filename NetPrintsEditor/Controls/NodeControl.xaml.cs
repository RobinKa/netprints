using NetPrintsEditor.Commands;
using NetPrintsEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for NodeControl.xaml
    /// </summary>
    public partial class NodeControl : UserControl
    {
        public static DependencyProperty NodeProperty = DependencyProperty.Register(
            nameof(NetPrints.Graph.Node), typeof(NodeVM), typeof(NodeControl));

        public NodeVM Node
        {
            get => GetValue(NodeProperty) as NodeVM;
            set => SetValue(NodeProperty, value);
        }

        public NodeControl()
        {
            InitializeComponent();
        }

        #region Dragging
        private bool dragging = false;
        private bool dragged = false;
        private Point dragMousePos;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            dragged = false;
            dragging = true;

            if (!Node.IsSelected)
            {
                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.SelectNode, Node);
            }

            Node.DragStart();

            dragMousePos = PointToScreen(e.GetPosition(this));

            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (dragging)
            {
                dragging = false;
                Node.DragEnd();

                ReleaseMouseCapture();
            }

            if (!dragged)
            {
                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.SelectNode, Node);
            }

            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (dragging)
            {
                Point mousePosition = PointToScreen(e.GetPosition(this));
                Vector offset = mousePosition - dragMousePos;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (offset.X != 0 && offset.Y != 0)
                    {
                        dragMousePos = mousePosition;

                        dragged = true;
                        Node.DragMove(offset.X, offset.Y);
                    }
                }
                else
                {
                    dragging = false;
                    Node.DragEnd();
                    ReleaseMouseCapture();
                }

                e.Handled = true;
            }
        }
        #endregion

        private void NodeVariants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Change overload to selected overload
            if (e.AddedItems.Count > 0)
            {
                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.ChangeNodeOverload, new NetPrintsCommands.ChangeNodeOverloadParameters
                (
                    Node,
                    e.AddedItems[0]
                ));
            }
        }

        private void OnLeftPinsPlusClicked(object sender, RoutedEventArgs e) => Node.LeftPinsPlusClicked();
        private void OnLeftPinsMinusClicked(object sender, RoutedEventArgs e) => Node.LeftPinsMinusClicked();
        private void OnRightPinsPlusClicked(object sender, RoutedEventArgs e) => Node.RightPinsPlusClicked();
        private void OnRightPinsMinusClicked(object sender, RoutedEventArgs e) => Node.RightPinsMinusClicked();
    }
}
