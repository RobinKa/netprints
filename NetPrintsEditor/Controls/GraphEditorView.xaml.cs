using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Dialogs;
using NetPrintsEditor.Reflection;
using NetPrintsEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for FunctionEditorControl.xaml
    /// </summary>
    public partial class GraphEditorView : UserControl
    {
        public Grid Grid => grid;
        public Canvas DrawCanvas => graphEditorWindow.drawCanvas;

        public const double GridCellSize = 28;

        public NodeGraphVM Graph
        {
            get => DataContext as NodeGraphVM;
            set => DataContext = value;
        }

        public GraphEditorView()
        {
            InitializeComponent();
        }

        public void ShowVariableGetSet(VariableSpecifier variableSpecifier, Point? position = null)
        {
            grid.ContextMenu.IsOpen = false;

            // Use current mouse position if position is not set
            Point pos = position ?? Mouse.GetPosition(drawCanvas);

            Func<TypeSpecifier, TypeSpecifier, bool> isSubclassOf = App.ReflectionProvider.TypeSpecifierIsSubclassOf;

            variableGetSet.VariableSpecifier = variableSpecifier;
            variableGetSet.CanGet = NetPrintsUtil.IsVisible(Graph.Class.Type, variableSpecifier.DeclaringType, variableSpecifier.GetterVisibility, isSubclassOf);
            variableGetSet.CanSet = NetPrintsUtil.IsVisible(Graph.Class.Type, variableSpecifier.DeclaringType, variableSpecifier.SetterVisibility, isSubclassOf);

            Canvas.SetLeft(variableGetSet, pos.X - variableGetSet.Width / 2);
            Canvas.SetTop(variableGetSet, pos.Y - variableGetSet.Height / 2);

            variableGetSet.Visibility = Visibility.Visible;
        }

        public void HideVariableGetSet()
        {
            variableGetSet.VariableSpecifier = null;
            variableGetSet.Visibility = Visibility.Hidden;
        }

        private void OnVariableGetSetMouseLeave(object sender, MouseEventArgs e)
        {
            HideVariableGetSet();
        }

        private void OnVariableSetClicked(VariableGetSetControl sender,
            VariableSpecifier variableSpecifier, bool wasSet)
        {
            Point position = Mouse.GetPosition(drawCanvas);

            if (wasSet)
            {
                // VariableSetterNode(Method method, TypeSpecifier targetType, 
                // string variableName, TypeSpecifier variableType) 

                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(VariableSetterNode), Graph.Graph, position.X, position.Y,
                    variableSpecifier
                ));
            }
            else
            {
                // VariableGetterNode(Method method, TypeSpecifier targetType, 
                // string variableName, TypeSpecifier variableType) 

                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(VariableGetterNode), Graph.Graph, position.X, position.Y,
                    variableSpecifier
                ));
            }

            HideVariableGetSet();
        }

        private void OnGridDrop(object sender, DragEventArgs e)
        {
            if (Graph != null)
            {
                if (e.Data.GetDataPresent(typeof(MemberVariableVM)))
                {
                    MemberVariableVM variable = e.Data.GetData(typeof(MemberVariableVM)) as MemberVariableVM;

                    ShowVariableGetSet(variable.Specifier, e.GetPosition(drawCanvas));

                    e.Handled = true;
                }
                else if (e.Data.GetDataPresent(typeof(NodePinVM)))
                {
                    Graph.SuggestionPin = ((NodePinVM)e.Data.GetData(typeof(NodePinVM))).Pin;
                    Graph.UpdateSuggestions();

                    // Open the context menu
                    grid.ContextMenu.PlacementTarget = grid;
                    grid.ContextMenu.IsOpen = true;

                    e.Handled = true;
                }
                else if (e.Data.GetDataPresent(typeof(NodeGraphVM)))
                {
                    Point mousePosition = e.GetPosition(graphEditorWindow);
                    NodeGraphVM method = e.Data.GetData(typeof(NodeGraphVM)) as NodeGraphVM;

                    if (method.IsConstructor)
                    {
                        // ConstructorNode(Method method, ConstructorSpecifier specifier)

                        // TODO: Get this from constructor directly somehow
                        // TODO: Get named type specifiers from method
                        ConstructorSpecifier constructorSpecifier = new ConstructorSpecifier(
                            method.NamedArgumentTypes.Select(nt => new MethodParameter(nt.Name, nt.Value, MethodParameterPassType.Default)),
                            method.Class.Type
                        );

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(ConstructorNode), Graph.Graph, mousePosition.X, mousePosition.Y,
                            constructorSpecifier
                        ));
                    }
                    else
                    {
                        // CallMethodNode(Method method, MethodSpecifier methodSpecifier, IList<BaseType> genericArgumentTypes)

                        // TODO: Get this from method directly somehow
                        // TODO: Get named type specifiers from method
                        MethodSpecifier methodSpecifier = new MethodSpecifier(method.Name,
                            method.NamedArgumentTypes.Select(nt => new MethodParameter(nt.Name, nt.Value, MethodParameterPassType.Default)),
                            method.ReturnTypes.Cast<TypeSpecifier>(),
                            method.Modifiers, method.Visibility,
                            method.Class.Type, Array.Empty<BaseType>());

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(CallMethodNode), Graph.Graph, mousePosition.X, mousePosition.Y,
                            methodSpecifier,
                            Array.Empty<GenericType>()
                        ));
                    }

                    e.Handled = true;
                }
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (Graph != null)
            {
                if (e.Data.GetDataPresent(typeof(MemberVariableVM)))
                {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                }
                else if (e.Data.GetDataPresent(typeof(NodePinVM)))
                {
                    // Set connecting position to the correct relative mouse position
                    NodePinVM pin = e.Data.GetData(typeof(NodePinVM)) as NodePinVM;
                    pin.ConnectingAbsolutePosition = e.GetPosition(drawCanvas);

                    e.Effects = DragDropEffects.Link;
                    e.Handled = true;
                }
                else if (e.Data.GetDataPresent(typeof(NodeGraphVM)))
                {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                }
            }
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Graph.SuggestionPin = null;
            Graph.UpdateSuggestions();
        }

        #region DrawCanvas dragging and scaling
        private bool dragCanvas = false;
        private Point dragCanvasStartLocation;
        private Vector dragCanvasStartOffset;
        private bool movedDuringDrag = false;

        private bool boxSelect = false;
        private Point boxSelectStartPoint;

        /// <summary>
        /// Transform scale of the canvas.
        /// </summary>
        public double DrawCanvasScale
        {
            get => drawCanvasScale;
            set
            {
                drawCanvasScale = value;
                if (Graph != null)
                {
                    Graph.NodeDragScale = 1 / drawCanvasScale;
                }
            }
        }

        private double drawCanvasScale = 1;
        private const double DrawCanvasMinScale = 0.3;
        private const double DrawCanvasMaxScale = 1.0;
        private const double DrawCanvasScaleFactor = 1.3;

        private void SelectWithinRectangle(Rect rectangle)
        {
            if (Graph != null)
            {
                var selectedNodes = new List<NodeVM>();

                for (int i = 0; i < nodeList.Items.Count; i++)
                {
                    // Check if the control intersects with the rectangle

                    var nodeControl = (ContentPresenter)nodeList.ItemContainerGenerator.ContainerFromIndex(i);
                    NodeVM node = (NodeVM)nodeControl.Content;

                    double nodeX = node.Node.PositionX;
                    double nodeY = node.Node.PositionY;
                    double nodeWidth = nodeControl.ActualWidth;
                    double nodeHeight = nodeControl.ActualHeight;

                    if (rectangle.IntersectsWith(new Rect(nodeX, nodeY, nodeWidth, nodeHeight)))
                    {
                        selectedNodes.Add(node);
                    }
                }

                Graph.SelectNodes(selectedNodes);
            }
        }

        private void OnDrawCanvasLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Deselect node
            if (Graph?.SelectedNodes != null)
            {
                Graph.DeselectNodes();
            }

            boxSelect = true;
            boxSelectStartPoint = e.GetPosition(drawCanvas);
            Canvas.SetLeft(boxSelectionBorder, boxSelectStartPoint.X);
            Canvas.SetTop(boxSelectionBorder, boxSelectStartPoint.Y);
            boxSelectionBorder.Width = 0;
            boxSelectionBorder.Height = 0;
            boxSelectionBorder.Visibility = Visibility.Visible;
            drawCanvas.CaptureMouse();

            e.Handled = true;
        }

        private void OnDrawCanvasLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (boxSelect)
            {
                boxSelect = false;
                drawCanvas.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
                boxSelectionBorder.Visibility = Visibility.Hidden;
                SelectWithinRectangle(new Rect(Canvas.GetLeft(boxSelectionBorder), Canvas.GetTop(boxSelectionBorder), boxSelectionBorder.Width, boxSelectionBorder.Height));
                e.Handled = movedDuringDrag;
            }
        }

        private void OnDrawCanvasRightMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragCanvas = true;
            dragCanvasStartLocation = e.GetPosition(this);
            dragCanvasStartOffset = drawCanvas.RenderTransform is TranslateTransform currentTransform ?
                    new Vector(currentTransform.X, currentTransform.Y) :
                    new Vector(0, 0);

            movedDuringDrag = false;

            drawCanvas.CaptureMouse();

            e.Handled = true;
        }

        private void OnDrawCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (dragCanvas)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    Vector offset = dragCanvasStartOffset + (e.GetPosition(this) - dragCanvasStartLocation);

                    drawCanvas.RenderTransform = new TranslateTransform(
                        Math.Round(offset.X),
                        Math.Round(offset.Y));

                    movedDuringDrag = true;
                    Mouse.OverrideCursor = Cursors.ScrollAll;
                }
                else
                {
                    dragCanvas = false;
                }

                e.Handled = true;
            }

            if (boxSelect)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    double x = boxSelectStartPoint.X;
                    double y = boxSelectStartPoint.Y;
                    double mx = e.GetPosition(drawCanvas).X;
                    double my = e.GetPosition(drawCanvas).Y;

                    double rectX = mx > x ? x : mx;
                    double rectY = my > y ? y : my;
                    double rectWidth = Math.Abs(x - mx);
                    double rectHeight = Math.Abs(y - my);

                    Canvas.SetLeft(boxSelectionBorder, rectX);
                    Canvas.SetTop(boxSelectionBorder, rectY);
                    boxSelectionBorder.Width = rectWidth;
                    boxSelectionBorder.Height = rectHeight;
                }
                else
                {
                    boxSelect = false;
                }

                e.Handled = true;
            }
        }

        private void OnDrawCanvasRightMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dragCanvas)
            {
                dragCanvas = false;
                drawCanvas.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
                e.Handled = movedDuringDrag;
            }
        }

        private void OnMouseWheelScroll(object sender, MouseWheelEventArgs e)
        {
            double oldScale = DrawCanvasScale;

            if (e.Delta < 0)
            {
                DrawCanvasScale /= DrawCanvasScaleFactor;
            }
            else
            {
                DrawCanvasScale *= DrawCanvasScaleFactor;
            }

            // Clamp scale between min and max
            if (DrawCanvasScale < DrawCanvasMinScale)
            {
                DrawCanvasScale = DrawCanvasMinScale;
            }
            else if (DrawCanvasScale > DrawCanvasMaxScale)
            {
                DrawCanvasScale = DrawCanvasMaxScale;
            }

            Vector mousePos = (Vector)e.GetPosition(drawCanvas);

            drawCanvas.LayoutTransform = new ScaleTransform(DrawCanvasScale, DrawCanvasScale);

            // Translate if the scale did not stay the same
            if (oldScale != DrawCanvasScale)
            {
                Vector currentOffset = drawCanvas.RenderTransform is TranslateTransform currentTransform ?
                    new Vector(currentTransform.X, currentTransform.Y) :
                    new Vector(0, 0);

                Vector targetOffset = currentOffset - Math.Sign(e.Delta) * (DrawCanvasScaleFactor - 1) * mousePos / 2;

                drawCanvas.RenderTransform = new TranslateTransform(
                    Math.Round(targetOffset.X),
                    Math.Round(targetOffset.Y));
            }

            e.Handled = true;
        }

        private void ResetDrawCanvasTransform()
        {
            DrawCanvasScale = 1;
            dragCanvas = false;

            drawCanvas.RenderTransform = new TranslateTransform(0, 0);
            drawCanvas.LayoutTransform = new ScaleTransform(1, 1);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DataContextProperty)
            {
                ResetDrawCanvasTransform();
            }
        }
        #endregion

        private void CablePath_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Path path && path.DataContext is NodePinVM pin && !pin.IsFaint)
            {
                path.Opacity = 1;
            }
        }

        private void CablePath_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Path path && path.DataContext is NodePinVM pin && !pin.IsFaint)
            {
                path.Opacity = 0.7;
            }
        }

        private void CablePath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (!(element?.DataContext is NodePinVM pin))
            {
                throw new Exception("Could not find cable's pin.");
            }

            if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                pin.AddRerouteNode();
            }
            else if (e.ChangedButton == MouseButton.Middle)
            {
                pin.DisconnectAll();
            }
        }

        private void CablePath_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (!(element?.DataContext is NodePinVM pin))
            {
                throw new Exception("Could not find cable's pin.");
            }

            if (e.ChangedButton == MouseButton.XButton1 && e.RightButton == MouseButtonState.Released)
            {
                pin.IsFaint = !pin.IsFaint;
            }
        }
    }
}
