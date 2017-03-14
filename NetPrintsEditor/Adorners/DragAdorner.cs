using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NetPrintsEditor.Adorners
{
    public class DragAdorner : Adorner
    {
        private bool dragging = false;
        private Point dragLastPosition;

        public event EventHandler OnDragStart;
        public event EventHandler OnDragEnd;

        public DragAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            adornedElement.MouseLeftButtonDown += AdornedElement_MouseLeftButtonDown;
            adornedElement.MouseLeftButtonUp += AdornedElement_MouseLeftButtonUp;
            adornedElement.MouseMove += AdornedElement_MouseMove;
        }

        private void AdornedElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point newPosition = e.GetPosition(null);

                var offset = newPosition - dragLastPosition;

                var transform = AdornedElement.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    AdornedElement.RenderTransform = transform;
                }

                transform.X += offset.X;
                transform.Y += offset.Y;

                dragLastPosition = newPosition;
            }
        }

        private void AdornedElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragging = true;
            dragLastPosition = e.GetPosition(null);

            AdornedElement.CaptureMouse();
            e.Handled = true;

            OnDragStart?.Invoke(this, new EventArgs());
        }

        private void AdornedElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;

            AdornedElement.ReleaseMouseCapture();
            e.Handled = true;

            OnDragEnd?.Invoke(this, new EventArgs());
        }
    }
}
