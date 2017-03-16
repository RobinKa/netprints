using NetPrints.Core;
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

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for MethodPropertyEditorControl.xaml
    /// </summary>
    public partial class MethodPropertyEditorControl : UserControl
    {
        public static DependencyProperty MethodProperty = DependencyProperty.Register(
            nameof(Method), typeof(Method), typeof(MethodPropertyEditorControl));

        public Method Method
        {
            get => GetValue(MethodProperty) as Method;
            set => SetValue(MethodProperty, value);
        }

        public MethodPropertyEditorControl()
        {
            InitializeComponent();
        }
    }
}
