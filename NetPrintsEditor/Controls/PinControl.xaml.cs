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

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for PinControl.xaml
    /// </summary>
    public partial class PinControl : UserControl
    {
        public static readonly DependencyProperty PinProperty =
            DependencyProperty.Register("Pin", typeof(NodePin), typeof(PinControl));

        public NodePin Pin
        {
            get => GetValue(PinProperty) as NodePin;
            set => SetValue(PinProperty, value);
        }

        /*public Point CableTarget
        {
            get
            {
                return curve.Points.Last();
            }
            set
            {
                curve.Points[1] = new Point((curve.Points[0].X + value.X) / 2, curve.Points[0].Y);
                curve.Points[2] = new Point(curve.Points[1].X, (curve.Points[0].Y + value.Y) / 2);
                curve.Points[3] = value;
            }
        }

        public bool CableVisible
        {
            get
            {
                return cable.IsVisible;
            }
            set
            {
                cable.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }*/

        public PinControl()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //CableVisible = true;

            //CaptureMouse();
            e.Handled = true;
        }
        /*
        public void ConnectToPinControl(PinControl other)
        {
            CableVisible = other != null;

            if(other != null)
            {
                CableTarget = TranslatePoint(other.RenderTransformOrigin, other);
            }
        }*/
    }
}
