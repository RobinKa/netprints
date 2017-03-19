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
using System.ComponentModel;
using NetPrints.Graph;
using NetPrintsEditor.ViewModels;
using NetPrintsEditor.Commands;
using static NetPrintsEditor.Commands.NetPrintsCommands;
using System.Threading;

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

        public PinControl FindPinControl(NodePin pin)
        {
            ItemsControl[] pinLists = new ItemsControl[]
            {
                inputDataPinList, outputDataPinList,
                inputExecPinList, outputExecPinList
            };

            DependencyObject foundItem = null;
            foreach(ItemsControl pinList in pinLists)
            {
                foundItem = pinList.ItemContainerGenerator.ContainerFromItem(pin);
                if (foundItem != null)
                    break;
            }

            if(foundItem == null)
            {
                return null;
            }

            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(foundItem); i++)
            {
                var v = VisualTreeHelper.GetChild(foundItem, i);
                if(v is PinControl pc && pc.Pin == pin)
                {
                    return pc;
                }
            }

            return null;
        }

        public NodeControl()
        {
            InitializeComponent();
        }

        #region Dragging
        private bool dragging = false;
        private Point dragStartMousePosition;
        private Point dragStartElementPosition;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            dragging = true;

            dragStartElementPosition = new Point(Node.PositionX, Node.PositionY);
            dragStartMousePosition = e.GetPosition(null);

            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (dragging)
            {
                dragging = false;

                ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (dragging)
            {
                Point mousePosition = e.GetPosition(null);

                Vector offset = mousePosition - dragStartMousePosition;
                
                Node.PositionX = dragStartElementPosition.X + offset.X;
                Node.PositionY = dragStartElementPosition.Y + offset.Y;

                Node.PositionX -= Node.PositionX % MethodEditorControl.GridCellSize;
                Node.PositionY -= Node.PositionY % MethodEditorControl.GridCellSize;

                InvalidateVisual();
            }
        }
        #endregion
    }
}
