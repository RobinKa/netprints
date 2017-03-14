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
using NetPrints.Graph;
using NetPrintsEditor.Commands;
using System.ComponentModel;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for PinControl.xaml
    /// </summary>
    public partial class PinControl : UserControl
    {
        private const double SplineAlpha = 0.5;

        public static readonly DependencyProperty StartPointProperty =
            DependencyProperty.Register(nameof(StartPoint), typeof(Point), typeof(PinControl));

        public static readonly DependencyProperty AnchorPointProperty =
            DependencyProperty.Register(nameof(AnchorPoint), typeof(Point), typeof(PinControl));

        public static readonly DependencyProperty ControlPoint1Property =
            DependencyProperty.Register(nameof(ControlPoint1), typeof(Point), typeof(PinControl));

        public static readonly DependencyProperty ControlPoint2Property =
           DependencyProperty.Register(nameof(ControlPoint2), typeof(Point), typeof(PinControl));

        public static readonly DependencyProperty ConnectedPinProperty =
           DependencyProperty.Register(nameof(ConnectedPin), typeof(PinControl), typeof(PinControl));

        public PinControl ConnectedPin
        {
            get
            {
                return (PinControl)GetValue(ConnectedPinProperty);
            }
            set
            {
                if(ConnectedPin != null)
                {
                    value.LayoutUpdated -= OnLayoutUpdated;
                }

                SetValue(ConnectedPinProperty, value);

                if (value != null)
                {
                    value.LayoutUpdated += OnLayoutUpdated;
                    value.InvalidateVisual();
                }

                InvalidateVisual();
            }
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            StartPoint = ellipse.TransformToVisual(canvas).Transform(new Point(
                    ellipse.RenderSize.Width / 2, ellipse.RenderSize.Height / 2));

            if (ConnectedPin != null && ConnectedPin.ellipse.FindCommonVisualAncestor(canvas) != null)
            {
                AnchorPoint = ConnectedPin.ellipse.TransformToVisual(canvas).Transform(new Point(
                    ConnectedPin.ellipse.RenderSize.Width / 2, ConnectedPin.ellipse.RenderSize.Height / 2));
            }
            else
            {
                AnchorPoint = StartPoint;
            }

            ControlPoint1 = new Point((1 - SplineAlpha) * AnchorPoint.X, 0);
            ControlPoint2 = new Point(SplineAlpha * AnchorPoint.X, AnchorPoint.Y);

            cable.Visibility = ConnectedPin != null ? Visibility.Visible : Visibility.Hidden;
        }

        public Point StartPoint
        {
            get => (Point)GetValue(StartPointProperty);
            set => SetValue(StartPointProperty, value);
        }

        public Point AnchorPoint
        {
            get => (Point)GetValue(AnchorPointProperty);
            set => SetValue(AnchorPointProperty, value);
        }

        public Point ControlPoint1
        {
            get => (Point)GetValue(ControlPoint1Property);
            set => SetValue(ControlPoint1Property, value);
        }

        public Point ControlPoint2
        {
            get => (Point)GetValue(ControlPoint2Property);
            set => SetValue(ControlPoint2Property, value);
        }

        public static readonly DependencyProperty PinProperty =
            DependencyProperty.Register("Pin", typeof(NodePin), typeof(PinControl));
        
        public NodePin Pin
        {
            get => GetValue(PinProperty) as NodePin;
            set => SetValue(PinProperty, value);
        }

        public PinControl()
        {
            InitializeComponent();
            LayoutUpdated += OnLayoutUpdated;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if(e.Property == PinProperty)
            {
                if(Pin is NodeInputDataPin || Pin is NodeInputExecPin)
                {
                    grid.ColumnDefinitions[0].Width = new GridLength(20, GridUnitType.Pixel);
                    grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    ellipse.SetValue(Grid.ColumnProperty, 0);
                    label.SetValue(Grid.ColumnProperty, 1);
                    label.HorizontalContentAlignment = HorizontalAlignment.Left;
                }
                else
                {
                    grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[1].Width = new GridLength(20, GridUnitType.Pixel);
                    ellipse.SetValue(Grid.ColumnProperty, 1);
                    label.SetValue(Grid.ColumnProperty, 0);
                    label.HorizontalContentAlignment = HorizontalAlignment.Right;
                }

                Color newColor = Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);

                if (Pin is NodeInputDataPin)
                {
                    newColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xFF);
                }
                else if(Pin is NodeOutputDataPin)
                {
                    newColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xFF);
                }
                else if(Pin is NodeInputExecPin)
                {
                    newColor = Color.FromArgb(0xFF, 0xE0, 0xFF, 0xE0);
                }
                else if(Pin is NodeOutputExecPin)
                {
                    newColor = Color.FromArgb(0xFF, 0xE0, 0xFF, 0xE0);
                }

                SolidColorBrush brush = new SolidColorBrush(newColor);
                ellipse.Fill = brush;
                cable.Stroke = brush;
            }
        }

        private void OnEllipseMouseMove(object sender, MouseEventArgs e)
        {
            if(sender is Ellipse el && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(el, this, DragDropEffects.Link);
            }
        }

        private void OnEllipseDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PinControl)))
            {
                PinControl droppedPinControl = e.Data.GetData(typeof(PinControl)) as PinControl;

                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.ConnectPins, new NetPrintsCommands.ConnectPinsParameters()
                {
                    PinA = droppedPinControl.Pin,
                    PinB = Pin
                });

                if(Pin is NodeInputDataPin || Pin is NodeOutputExecPin)
                {
                    ConnectedPin = droppedPinControl;
                }
                else
                {
                    droppedPinControl.ConnectedPin = this;
                }
            }
        }

        private void OnEllipseDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (e.Data.GetDataPresent(typeof(PinControl)))
            {
                PinControl draggingPinControl = e.Data.GetData(typeof(PinControl)) as PinControl;
                
                if(GraphUtil.CanConnectNodePins(draggingPinControl.Pin, Pin))
                {
                    e.Effects = DragDropEffects.Link;
                }
            }
        }

        // Needed so dragging doesnt happen
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
