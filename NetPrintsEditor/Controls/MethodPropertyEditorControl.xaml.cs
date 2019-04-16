using NetPrintsEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for MethodPropertyEditorControl.xaml
    /// </summary>
    public partial class MethodPropertyEditorControl : UserControl
    {
        public static DependencyProperty MethodProperty = DependencyProperty.Register(
            nameof(Method), typeof(MethodVM), typeof(MethodPropertyEditorControl));

        public MethodVM Method
        {
            get => GetValue(MethodProperty) as MethodVM;
            set => SetValue(MethodProperty, value);
        }

        public MethodPropertyEditorControl()
        {
            InitializeComponent();
        }
    }
}
