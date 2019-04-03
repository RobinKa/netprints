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
    public partial class MethodEditorControl : UserControl
    {
        public const double GridCellSize = 28;

        public MethodVM Method
        {
            get => GetValue(MethodProperty) as MethodVM;
            set => SetValue(MethodProperty, value);
        }

        public static DependencyProperty MethodProperty = DependencyProperty.Register(
            nameof(Method), typeof(MethodVM), typeof(MethodEditorControl));
        
        public static DependencyProperty SuggestionsProperty = DependencyProperty.Register(
            nameof(Suggestions), typeof(IEnumerable<object>), typeof(MethodEditorControl));

        public IEnumerable<object> Suggestions
        {
            get => (IEnumerable<object>)GetValue(SuggestionsProperty);
            set => SetValue(SuggestionsProperty, value.ToList());
        }

        /// <summary>
        /// Pin that was dragged to generate suggestions.
        /// Null if that suggestions were not created for a pin.
        /// </summary>
        public NodePinVM SuggestionPin
        {
            get;
            set;
        }

        private List<object> builtInNodes = new List<object>() {
            TypeSpecifier.FromType<ForLoopNode>(),
            TypeSpecifier.FromType<IfElseNode>(),
            TypeSpecifier.FromType<ConstructorNode>(),
            TypeSpecifier.FromType<TypeOfNode>(),
            TypeSpecifier.FromType<ExplicitCastNode>(),
            TypeSpecifier.FromType<ReturnNode>(),
            TypeSpecifier.FromType<MakeArrayNode>(),
            TypeSpecifier.FromType<LiteralNode>(),
            TypeSpecifier.FromType<TypeNode>(),
            TypeSpecifier.FromType<MakeArrayTypeNode>(),
        };

        public MethodEditorControl()
        {
            InitializeComponent();
        }

        public void ShowVariableGetSet(VariableSpecifier variableSpecifier, Point? position = null)
        {
            grid.ContextMenu.IsOpen = false;

            // Use current mouse position if position is not set
            Point pos = position ?? Mouse.GetPosition(drawCanvas);

            Func<TypeSpecifier, TypeSpecifier, bool> isSubclassOf = ProjectVM.Instance.ReflectionProvider.TypeSpecifierIsSubclassOf;

            variableGetSet.VariableSpecifier = variableSpecifier;
            variableGetSet.CanGet = NetPrintsUtil.IsVisible(Method.Class.Type, variableSpecifier.DeclaringType, variableSpecifier.GetterVisibility, isSubclassOf);
            variableGetSet.CanSet = NetPrintsUtil.IsVisible(Method.Class.Type, variableSpecifier.DeclaringType, variableSpecifier.SetterVisibility, isSubclassOf);

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
                    typeof(VariableSetterNode), Method.Method, position.X, position.Y,
                    variableSpecifier
                ));
            }
            else
            {
                // VariableGetterNode(Method method, TypeSpecifier targetType, 
                // string variableName, TypeSpecifier variableType) 

                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(VariableGetterNode), Method.Method, position.X, position.Y,
                    variableSpecifier
                ));
            }

            HideVariableGetSet();
        }

        private void OnGridDrop(object sender, DragEventArgs e)
        {
            if (Method != null)
            {
                if (e.Data.GetDataPresent(typeof(VariableVM)))
                {
                    VariableVM variable = e.Data.GetData(typeof(VariableVM)) as VariableVM;

                    ShowVariableGetSet(variable.Specifier, e.GetPosition(drawCanvas));

                    e.Handled = true;
                }
                else if (e.Data.GetDataPresent(typeof(NodePinVM)))
                {
                    // Show all relevant methods for the type of the pin if its a data pin

                    NodePinVM pin = e.Data.GetData(typeof(NodePinVM)) as NodePinVM;
                    SuggestionPin = pin;

                    if (pin.Pin is NodeOutputDataPin odp)
                    {
                        if (odp.PinType.Value is TypeSpecifier pinTypeSpec)
                        {
                            // Add make delegate
                            IEnumerable<object> suggestions = new object[] { new MakeDelegateTypeInfo(pinTypeSpec, Method.Class.Type) };

                            // Add properties and methods of the pin type
                            suggestions = suggestions.Concat(ProjectVM.Instance.ReflectionProvider.GetVariables(
                                new ReflectionProviderVariableQuery()
                                    .WithType(pinTypeSpec)
                                    .WithVisibleFrom(Method.Class.Type)
                                    .WithStatic(false)));

                            suggestions = suggestions.Concat(ProjectVM.Instance.ReflectionProvider.GetMethods(
                                new ReflectionProviderMethodQuery()
                                    .WithVisibleFrom(Method.Class.Type)
                                    .WithStatic(false)
                                    .WithType(pinTypeSpec)));

                            // Add methods of the super type that can accept the pin type as argument
                            suggestions = suggestions.Concat(ProjectVM.Instance.ReflectionProvider.GetMethods(
                                new ReflectionProviderMethodQuery()
                                    .WithVisibleFrom(Method.Class.Type)
                                    .WithStatic(false)
                                    .WithArgumentType(pinTypeSpec)
                                    .WithType(Method.Class.SuperType)));

                            // Add static functions taking the type of the pin
                            suggestions = suggestions.Concat(ProjectVM.Instance.ReflectionProvider.GetMethods(
                                new ReflectionProviderMethodQuery()
                                    .WithArgumentType(pinTypeSpec)
                                    .WithVisibleFrom(Method.Class.Type)
                                    .WithStatic(true)));

                            Suggestions = suggestions.Distinct();
                        }
                    }
                    else if (pin.Pin is NodeInputDataPin idp)
                    {
                        if (idp.PinType.Value is TypeSpecifier pinTypeSpec)
                        {
                            // Properties of base class that inherit from needed type
                            IEnumerable<object> baseProperties = ProjectVM.Instance.ReflectionProvider.GetVariables(
                                new ReflectionProviderVariableQuery()
                                    .WithType(Method.Class.SuperType)
                                    .WithVisibleFrom(Method.Class.Type)
                                    .WithVariableType(pinTypeSpec, true));

                            Suggestions = baseProperties
                                .Concat(ProjectVM.Instance.ReflectionProvider.GetMethods(
                                    new ReflectionProviderMethodQuery()
                                        .WithStatic(true)
                                        .WithVisibleFrom(Method.Class.Type)
                                        .WithReturnType(pinTypeSpec)))
                                .Distinct();
                        }
                    }
                    else if (pin.Pin is NodeOutputExecPin oxp)
                    {
                        pin.ConnectedPin = null;

                        Suggestions = builtInNodes
                            .Concat(ProjectVM.Instance.ReflectionProvider.GetMethods(
                                new ReflectionProviderMethodQuery()
                                    .WithType(Method.Class.SuperType)
                                    .WithStatic(false)
                                    .WithVisibleFrom(Method.Class.Type)))
                            .Concat(ProjectVM.Instance.ReflectionProvider.GetMethods(
                                new ReflectionProviderMethodQuery()
                                .WithStatic(true)
                                .WithVisibleFrom(Method.Class.Type)))
                            .Concat(ProjectVM.Instance.ReflectionProvider.GetVariables(
                                new ReflectionProviderVariableQuery()
                                    .WithStatic(true)
                                    .WithVisibleFrom(Method.Class.Type)))
                            .Distinct();
                    }
                    else if (pin.Pin is NodeInputExecPin ixp)
                    {
                        Suggestions = builtInNodes
                            .Concat(ProjectVM.Instance.ReflectionProvider.GetMethods(
                                new ReflectionProviderMethodQuery()
                                    .WithType(Method.Class.SuperType)
                                    .WithStatic(false)
                                    .WithVisibleFrom(Method.Class.Type)))
                            .Concat(ProjectVM.Instance.ReflectionProvider.GetMethods(
                                new ReflectionProviderMethodQuery()
                                    .WithStatic(true)
                                    .WithVisibleFrom(Method.Class.Type)))
                            .Concat(ProjectVM.Instance.ReflectionProvider.GetVariables(
                                new ReflectionProviderVariableQuery()
                                    .WithStatic(true)
                                    .WithVisibleFrom(Method.Class.Type)))
                            .Distinct();
                    }
                    else if (pin.Pin is NodeInputTypePin itp)
                    {
                        Suggestions = ProjectVM.Instance.ReflectionProvider.GetNonStaticTypes();
                    }
                    else if (pin.Pin is NodeOutputTypePin otp)
                    {
                        IEnumerable<object> suggestions = new object[0];

                        if (otp.InferredType.Value is TypeSpecifier typeSpecifier)
                        {
                            suggestions = suggestions.Concat(ProjectVM.Instance.ReflectionProvider
                                .GetMethods(
                                    new ReflectionProviderMethodQuery()
                                        .WithType(typeSpecifier)
                                        .WithStatic(true)
                                        .WithVisibleFrom(Method.Class.Type)));
                        }

                        // Types with type parameters
                        suggestions = suggestions.Concat(ProjectVM.Instance.ReflectionProvider.GetNonStaticTypes()
                            .Where(t => t.GenericArguments.Any()));

                        // Public static methods that have type parameters
                        suggestions = suggestions.Concat(ProjectVM.Instance.ReflectionProvider
                            .GetMethods(
                                new ReflectionProviderMethodQuery()
                                    .WithStatic(true)
                                    .WithHasGenericArguments(true)
                                    .WithVisibleFrom(Method.Class.Type)));

                        Suggestions = suggestions.Distinct();
                    }
                    else
                    {
                        // Unknown type, no suggestions
                        Suggestions = new object[0];
                    }

                    // Open the context menu
                    grid.ContextMenu.PlacementTarget = grid;
                    grid.ContextMenu.IsOpen = true;

                    e.Handled = true;
                }
                else if (e.Data.GetDataPresent(typeof(MethodVM)))
                {
                    Point mousePosition = e.GetPosition(methodEditorWindow);
                    MethodVM method = e.Data.GetData(typeof(MethodVM)) as MethodVM;

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
                            typeof(ConstructorNode), Method.Method, mousePosition.X, mousePosition.Y,
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
                            typeof(CallMethodNode), Method.Method, mousePosition.X, mousePosition.Y,
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

            if (Method != null)
            {
                if (e.Data.GetDataPresent(typeof(VariableVM)))
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
                else if (e.Data.GetDataPresent(typeof(MethodVM)))
                {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                }
            }
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (Method != null)
            {
                // Get properties and methods of base class.
                IEnumerable<object> baseProperties = ProjectVM.Instance.ReflectionProvider.GetVariables(
                    new ReflectionProviderVariableQuery()
                        .WithVisibleFrom(Method.Class.Type)
                        .WithType(Method.Class.SuperType)
                        .WithStatic(false));

                IEnumerable<object> baseMethods = ProjectVM.Instance.ReflectionProvider.GetMethods(
                    new ReflectionProviderMethodQuery()
                        .WithType(Method.Class.SuperType)
                        .WithVisibleFrom(Method.Class.Type)
                        .WithStatic(false));

                Suggestions = builtInNodes
                    .Concat(baseProperties)
                    .Concat(baseMethods)
                    .Concat(ProjectVM.Instance.ReflectionProvider.GetMethods(
                        new ReflectionProviderMethodQuery()
                            .WithStatic(true)
                            .WithVisibleFrom(Method.Class.Type)))
                    .Concat(ProjectVM.Instance.ReflectionProvider.GetVariables(
                            new ReflectionProviderVariableQuery()
                                .WithStatic(true)
                                .WithVisibleFrom(Method.Class.Type)))
                    .Distinct();
            }
            else
            {
                Suggestions = new object[0];
            }

            SuggestionPin = null;
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
                if (Method != null)
                {
                    Method.NodeDragScale = 1 / drawCanvasScale;
                }
            }
        }

        private double drawCanvasScale = 1;
        private const double DrawCanvasMinScale = 0.3;
        private const double DrawCanvasMaxScale = 1.0;
        private const double DrawCanvasScaleFactor = 1.3;

        private void SelectWithinRectangle(Rect rectangle)
        {
            if (Method != null)
            {
                var selectedNodes = new List<NodeVM>();

                for (int i = 0; i < nodeList.Items.Count; i++)
                {
                    // Check if the control intersects with the rectangle

                    var nodeControl = (ContentPresenter)nodeList.ItemContainerGenerator.ContainerFromIndex(i);
                    NodeVM node = (NodeVM)nodeControl.Content;

                    double nodeX = node.PositionX;
                    double nodeY = node.PositionY;
                    double nodeWidth = nodeControl.ActualWidth;
                    double nodeHeight = nodeControl.ActualHeight;

                    if (rectangle.IntersectsWith(new Rect(nodeX, nodeY, nodeWidth, nodeHeight)))
                    {
                        selectedNodes.Add(node);
                    }
                }

                Method.SelectedNodes = selectedNodes;
            }
        }

        private void OnDrawCanvasLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Deselect node
            if (Method?.SelectedNodes != null)
            {
                Method.SelectedNodes = null;
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
            TranslateTransform currentTransform = drawCanvas.RenderTransform as TranslateTransform;
            dragCanvas = true;
            dragCanvasStartLocation = e.GetPosition(this);
            dragCanvasStartOffset = currentTransform != null ?
                    new Vector(currentTransform.X, currentTransform.Y) :
                    new Vector(0, 0);

            movedDuringDrag = false;

            drawCanvas.CaptureMouse();
            
            e.Handled = true;
        }

        private void OnDrawCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if(dragCanvas)
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
                TranslateTransform currentTransform = drawCanvas.RenderTransform as TranslateTransform;
                Vector currentOffset = currentTransform != null ?
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

            if(e.Property == MethodProperty)
            {
                ResetDrawCanvasTransform();
            }
        }
        #endregion

        private void CablePath_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Path path)
            {
                path.Opacity = 1;
            }
        }

        private void CablePath_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Path path)
            {
                path.Opacity = 0.7;
            }
        }

        private void CablePath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            var pin = element?.DataContext as NodePinVM;

            if (pin == null)
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
    }
}
