using NetPrints.Graph;
using NetPrintsEditor.Commands;
using NetPrintsEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for PinControl.xaml
    /// </summary>
    public partial class PinControl : UserControl
    {
        public static readonly DependencyProperty ParentNodeControlProperty = DependencyProperty.Register(
            nameof(ParentNodeControl), typeof(NodeControl), typeof(PinControl));

        public NodeControl ParentNodeControl
        {
            get => (NodeControl)GetValue(ParentNodeControlProperty);
            set => SetValue(ParentNodeControlProperty, value);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            if (Pin != null)
            {
                Pin.PositionX = connector.Width / 2;
                Pin.PositionY = connector.Height / 2;

                if (ParentNodeControl != null && connector.FindCommonVisualAncestor(ParentNodeControl) != null)
                {
                    Pin.NodeRelativePosition = connector.TransformToVisual(ParentNodeControl).Transform(Pin.Position);
                }
            }
        }

        public NodePinVM Pin
        {
            get => DataContext as NodePinVM;
            set => DataContext = value;
        }

        public PinControl()
        {
            InitializeComponent();
            LayoutUpdated += OnLayoutUpdated;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DataContextProperty && Pin != null)
            {
                if (Pin.Pin is NodeInputDataPin || Pin.Pin is NodeInputExecPin || Pin.Pin is NodeInputTypePin)
                {
                    grid.ColumnDefinitions[0].Width = new GridLength(20, GridUnitType.Pixel);
                    grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                    connector.SetValue(Grid.ColumnProperty, 0);

                    labelContainer.SetValue(Grid.ColumnProperty, 2);
                    label.HorizontalContentAlignment = HorizontalAlignment.Left;
                    editableLabel.HorizontalContentAlignment = HorizontalAlignment.Left;
                }
                else
                {
                    grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[2].Width = new GridLength(20, GridUnitType.Pixel);
                    connector.SetValue(Grid.ColumnProperty, 2);

                    labelContainer.SetValue(Grid.ColumnProperty, 0);
                    label.HorizontalContentAlignment = HorizontalAlignment.Right;
                    editableLabel.HorizontalAlignment = HorizontalAlignment.Right;
                }
            }
        }

        private void OnPinElementMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Shape shape && e.LeftButton == MouseButtonState.Pressed)
            {
                Pin.IsBeingConnected = true;
                DragDrop.DoDragDrop(shape, Pin, DragDropEffects.Link);
                Pin.IsBeingConnected = false;
                e.Handled = true;
            }
        }

        private void OnPinElementDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NodePinVM)))
            {
                // Another pin was dropped on this pin, link it
                NodePinVM droppedPin = (NodePinVM)e.Data.GetData(typeof(NodePinVM));

                droppedPin.ConnectTo(Pin);

                e.Handled = true;
            }
        }

        private void OnPinElementDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (e.Data.GetDataPresent(typeof(NodePinVM)))
            {
                // Another pin is being hovered over this one, see if it can be linked to this pin

                NodePinVM draggingPin = (NodePinVM)e.Data.GetData(typeof(NodePinVM));

                if (GraphUtil.CanConnectNodePins(draggingPin.Pin, Pin.Pin,
                    (a, b) => App.ReflectionProvider.TypeSpecifierIsSubclassOf(a, b),
                    (a, b) => App.ReflectionProvider.HasImplicitCast(a, b)))
                {
                    e.Effects = DragDropEffects.Link;

                    draggingPin.ConnectingAbsolutePosition = Pin.AbsolutePosition;
                }
                else
                {
                    draggingPin.ConnectingAbsolutePosition = Pin.AbsolutePosition - (Vector)Pin.Position
                        + (Vector)e.GetPosition(this);
                }

                e.Handled = true;
            }
        }

        // Needed so dragging doesnt happen
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Disconnect the pin on middle click
            if (e.ChangedButton == MouseButton.Middle)
            {
                Pin.DisconnectAll();
                e.Handled = true;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.XButton1 && Pin.IsConnected)
            {
                Pin.IsFaint = !Pin.IsFaint;
                e.Handled = true;
            }
        }

        private void OnClearUnconnectedValue(object sender, MouseButtonEventArgs e)
        {
            Pin.ClearUnconnectedValue();
        }
    }
}
