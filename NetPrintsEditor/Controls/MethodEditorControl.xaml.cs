using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Dialogs;
using NetPrintsEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for FunctionEditorControl.xaml
    /// </summary>
    public partial class MethodEditorControl : UserControl
    {
        public const double GridCellSize = 20;

        public MethodVM Method
        {
            get => GetValue(MethodProperty) as MethodVM;
            set => SetValue(MethodProperty, value);
        }

        public static DependencyProperty MethodProperty = DependencyProperty.Register(
            nameof(Method), typeof(MethodVM), typeof(MethodEditorControl));
        
        public static DependencyProperty SuggestionsProperty = DependencyProperty.Register(
            nameof(Suggestions), typeof(ObservableRangeCollection<object>), typeof(MethodEditorControl));

        public ObservableRangeCollection<object> Suggestions
        {
            get => (ObservableRangeCollection<object>)GetValue(SuggestionsProperty);
            set => SetValue(SuggestionsProperty, value);
        }

        public MethodEditorControl()
        {
            InitializeComponent();
        }

        public void ShowVariableGetSet(VariableGetSetInfo variableInfo, Point? position = null)
        {
            // Check that the tag is unused
            if(variableInfo.Tag != null)
            {
                throw new ArgumentException("variableInfo needs to have its Tag set to null because it is used for position");
            }

            // Use current mouse position if position is not set
            Point pos = position ?? Mouse.GetPosition(drawCanvas);

            variableInfo.Tag = pos;
            variableGetSet.VariableInfo = variableInfo;

            Canvas.SetLeft(variableGetSet, pos.X - variableGetSet.Width / 2);
            Canvas.SetTop(variableGetSet, pos.Y - variableGetSet.Height / 2);

            variableGetSet.Visibility = Visibility.Visible;
        }

        public void HideVariableGetSet()
        {
            variableGetSet.VariableInfo = null;
            variableGetSet.Visibility = Visibility.Hidden;
        }

        private void OnVariableGetSetMouseLeave(object sender, MouseEventArgs e)
        {
            HideVariableGetSet();
        }

        private void OnVariableSetClicked(VariableGetSetControl sender,
            VariableGetSetInfo variableInfo, bool wasSet)
        {
            Point position;

            // Try to get the spawn position from the variableInfo's Tag
            // Otherwise use current mouse location

            if(variableInfo.Tag is Point infoPosition)
            {
                position = infoPosition;
            }
            else
            {
                position = Mouse.GetPosition(drawCanvas);
            }

            if (wasSet)
            {
                // VariableSetterNode(Method method, TypeSpecifier targetType, 
                // string variableName, TypeSpecifier variableType) 

                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(VariableSetterNode), Method.Method, position.X, position.Y,
                    variableInfo.TargetType, variableInfo.Name, variableInfo.Type
                ));
            }
            else
            {
                // VariableGetterNode(Method method, TypeSpecifier targetType, 
                // string variableName, TypeSpecifier variableType) 

                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(VariableGetterNode), Method.Method, position.X, position.Y,
                    variableInfo.TargetType, variableInfo.Name, variableInfo.Type
                ));
            }

            HideVariableGetSet();
        }

        private void OnGridDrop(object sender, DragEventArgs e)
        {
            if (Method != null && e.Data.GetDataPresent(typeof(VariableVM)))
            {
                VariableVM variable = e.Data.GetData(typeof(VariableVM)) as VariableVM;

                bool canSet = !(variable.Modifiers.HasFlag(VariableModifiers.ReadOnly) ||
                    variable.Modifiers.HasFlag(VariableModifiers.Const));

                VariableGetSetInfo variableInfo = new VariableGetSetInfo(
                    variable.Name, variable.VariableType, true, canSet, Method.Class.Type);

                ShowVariableGetSet(variableInfo, e.GetPosition(drawCanvas));

                e.Handled = true;
            }
            else if(e.Data.GetDataPresent(typeof(NodePinVM)))
            {
                // Show all relevant methods for the type of the pin if its a data pin

                NodePinVM pin = e.Data.GetData(typeof(NodePinVM)) as NodePinVM;
                
                if (pin.Pin is NodeOutputDataPin odp)
                {
                    if (odp.PinType is TypeSpecifier pinTypeSpec)
                    {
                        // Add public methods
                        Suggestions = new ObservableRangeCollection<object>(
                            ProjectVM.Instance.ReflectionProvider.GetPublicMethodsForType(pinTypeSpec));

                        if (!pinTypeSpec.IsInterface && !pinTypeSpec.IsEnum)
                        {
                            // Add properties
                            Suggestions.AddRange(ProjectVM.Instance.ReflectionProvider.GetPublicPropertiesForType(
                                pinTypeSpec));
                        }

                        // Add static functions taking the type of the pin
                        Suggestions.AddRange(ProjectVM.Instance.ReflectionProvider.GetStaticFunctionsWithArgumentType(
                            pinTypeSpec));

                        // Add make delegate
                        Suggestions.Add(new MakeDelegateTypeInfo(pinTypeSpec));
                    }
                }
                else if (pin.Pin is NodeInputDataPin idp)
                {
                    if (idp.PinType is TypeSpecifier pinTypeSpec)
                    {
                        Suggestions = new ObservableRangeCollection<object>(
                            ProjectVM.Instance.ReflectionProvider.GetStaticFunctionsWithReturnType(
                                pinTypeSpec));
                    }
                }
                else if (pin.Pin is NodeOutputExecPin oxp)
                {
                    pin.ConnectedPin = null;

                    Suggestions = new ObservableRangeCollection<object>(ProjectVM.Instance.ReflectionProvider.
                        GetPublicMethodsForType(Method.Class.SuperType))
                    {
                        TypeSpecifier.Create<ForLoopNode>(),
                        TypeSpecifier.Create<IfElseNode>(),
                        TypeSpecifier.Create<ConstructorNode>(),
                    };
                }
                else if(pin.Pin is NodeInputExecPin ixp)
                {
                    Suggestions = new ObservableRangeCollection<object>(
                        ProjectVM.Instance.ReflectionProvider.GetStaticFunctions())
                    {
                        TypeSpecifier.Create<ForLoopNode>(),
                        TypeSpecifier.Create<IfElseNode>(),
                        TypeSpecifier.Create<ConstructorNode>(),
                    };
                }
                else
                {
                    // Unknown type, no suggestions
                    Suggestions = new ObservableRangeCollection<object>();
                }
                
                // Open the context menu
                grid.ContextMenu.PlacementTarget = grid;
                grid.ContextMenu.IsOpen = true;

                e.Handled = true;
            }
            if (Method != null && e.Data.GetDataPresent(typeof(MethodVM)))
            {
                Point mousePosition = e.GetPosition(methodEditorWindow);
                MethodVM method = e.Data.GetData(typeof(MethodVM)) as MethodVM;

                // CallMethodNode(Method method, MethodSpecifier methodSpecifier)

                // TODO: Get this from method directly somehow
                MethodSpecifier methodSpecifier = new MethodSpecifier(method.Name, 
                    method.ArgumentTypes.Cast<TypeSpecifier>(), 
                    method.ReturnTypes.Cast<TypeSpecifier>(),
                    method.Modifiers, method.Class.Type,
                    Array.Empty<BaseType>());

                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(CallMethodNode), Method.Method, mousePosition.X, mousePosition.Y,
                    methodSpecifier,
                    Array.Empty<GenericType>()
                ));

                e.Handled = true;
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (Method != null && e.Data.GetDataPresent(typeof(VariableVM)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else if(e.Data.GetDataPresent(typeof(NodePinVM)))
            {
                // Set connecting position to the correct relative mouse position
                NodePinVM pin = e.Data.GetData(typeof(NodePinVM)) as NodePinVM;
                pin.ConnectingAbsolutePosition = e.GetPosition(drawCanvas);

                e.Effects = DragDropEffects.Link;
                e.Handled = true;
            }
            else if(Method != null && e.Data.GetDataPresent(typeof(MethodVM)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (Method != null)
            {
                Suggestions = new ObservableRangeCollection<object>(ProjectVM.Instance.ReflectionProvider.
                    GetStaticFunctions())
                {
                    TypeSpecifier.Create<ForLoopNode>(),
                    TypeSpecifier.Create<IfElseNode>(),
                    TypeSpecifier.Create<ConstructorNode>(),
                };
            }
            else
            {
                Suggestions?.Clear();
            }
        }
        
        #region Commands
        private void OpenVariableGetSetCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is VariableGetSetInfo variableInfo;
        }

        private void OpenVariableGetSetExecute(object sender, ExecutedRoutedEventArgs e)
        {
            grid.ContextMenu.IsOpen = false;
            ShowVariableGetSet((VariableGetSetInfo)e.Parameter);
        }
        #endregion

        #region DrawCanvas dragging and scaling
        private bool dragCanvas = false;
        private Point dragCanvasStartLocation;
        private Vector dragCanvasStartOffset;

        private double drawCanvasScale = 1;
        private const double DrawCanvasMinScale = 0.3;
        private const double DrawCanvasMaxScale = 1.0;
        private const double DrawCanvasScaleFactor = 1.3;

        private void OnDrawCanvasLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Deselect node
            if (Method?.SelectedNode != null)
            {
                Method.SelectedNode = null;
                e.Handled = true;
            }
            
            TranslateTransform currentTransform = drawCanvas.RenderTransform as TranslateTransform;
            dragCanvas = true;
            dragCanvasStartLocation = e.GetPosition(this);
            dragCanvasStartOffset = currentTransform != null ?
                    new Vector(currentTransform.X, currentTransform.Y) :
                    new Vector(0, 0);

            drawCanvas.CaptureMouse();
            Mouse.OverrideCursor = Cursors.ScrollAll;
            e.Handled = true;
        }

        private void OnDrawCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if(dragCanvas)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Vector offset = dragCanvasStartOffset + (e.GetPosition(this) - dragCanvasStartLocation);

                    drawCanvas.RenderTransform = new TranslateTransform(
                        Math.Round(offset.X),
                        Math.Round(offset.Y));
                }
                else
                {
                    dragCanvas = false;
                }

                e.Handled = true;
            }
        }

        private void OnDrawCanvasLeftMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dragCanvas)
            {
                dragCanvas = false;
                drawCanvas.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
                e.Handled = true;
            }
        }

        private void OnMouseWheelScroll(object sender, MouseWheelEventArgs e)
        {
            double oldScale = drawCanvasScale;

            if (e.Delta < 0)
            {
                drawCanvasScale /= DrawCanvasScaleFactor;
            }
            else
            {
                drawCanvasScale *= DrawCanvasScaleFactor;
            }

            // Clamp scale between min and max
            if (drawCanvasScale < DrawCanvasMinScale)
            {
                drawCanvasScale = DrawCanvasMinScale;
            }
            else if (drawCanvasScale > DrawCanvasMaxScale)
            {
                drawCanvasScale = DrawCanvasMaxScale;
            }

            drawCanvas.LayoutTransform = new ScaleTransform(drawCanvasScale, drawCanvasScale);

            // Translate if the scale did not stay the same
            if (oldScale != drawCanvasScale)
            {
                TranslateTransform currentTransform = drawCanvas.RenderTransform as TranslateTransform;
                Vector currentOffset = currentTransform != null ?
                    new Vector(currentTransform.X, currentTransform.Y) :
                    new Vector(0, 0);

                Vector targetOffset = 0.8 * currentOffset - Math.Sign(e.Delta) * 0.2 * (Vector)e.GetPosition(drawCanvas);
                drawCanvas.RenderTransform = new TranslateTransform(
                    Math.Round(targetOffset.X),
                    Math.Round(targetOffset.Y));
            }

            e.Handled = true;
        }

        private void ResetDrawCanvasTransform()
        {
            drawCanvasScale = 1;
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
    }
}
