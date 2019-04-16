using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NetPrintsEditor.Adorners
{
    /// <summary>
    /// Adorner for dragging UI elements with a mouse.
    /// </summary>
    public class DragAdorner : Adorner
    {
        private bool dragging = false;
        private Point dragStartMousePosition;
        private Point dragStartElementPosition;

        /// <summary>
        /// Called when the dragging starts.
        /// </summary>
        public event EventHandler OnDragStart;

        /// <summary>
        /// Called when the dragging ends.
        /// </summary>
        public event EventHandler OnDragEnd;

        /// <summary>
        /// Size of a cell in the grid the UI element is movable on.
        /// </summary>
        public double CellSize
        {
            get;
            set;
        }

        public DragAdorner(UIElement adornedElement, double cellSize)
            : base(adornedElement)
        {
            adornedElement.MouseLeftButtonDown += AdornedElement_MouseLeftButtonDown;
            adornedElement.MouseLeftButtonUp += AdornedElement_MouseLeftButtonUp;
            adornedElement.MouseMove += AdornedElement_MouseMove;

            CellSize = cellSize;
        }

        private void AdornedElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point mousePosition = e.GetPosition(null);

                Vector offset = mousePosition - dragStartMousePosition;

                TranslateTransform transform = AdornedElement.RenderTransform as TranslateTransform;

                transform.X = dragStartElementPosition.X + offset.X;
                transform.Y = dragStartElementPosition.Y + offset.Y;

                transform.X -= transform.X % CellSize;
                transform.Y -= transform.Y % CellSize;

                AdornedElement.InvalidateVisual();
            }
        }

        private void AdornedElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragging = true;

            if (!(AdornedElement.RenderTransform is TranslateTransform transform))
            {
                transform = new TranslateTransform();
                AdornedElement.RenderTransform = transform;
            }

            dragStartElementPosition = new Point(transform.X, transform.Y);
            dragStartMousePosition = e.GetPosition(null);

            AdornedElement.CaptureMouse();
            e.Handled = true;

            OnDragStart?.Invoke(this, EventArgs.Empty);
        }

        private void AdornedElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dragging)
            {
                dragging = false;

                AdornedElement.ReleaseMouseCapture();
                e.Handled = true;

                OnDragEnd?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
