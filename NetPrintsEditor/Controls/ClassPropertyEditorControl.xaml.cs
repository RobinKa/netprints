using NetPrintsEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for VariableEditorControl.xaml
    /// </summary>
    public partial class ClassPropertyEditorControl : UserControl
    {
        public static DependencyProperty ClassProperty = DependencyProperty.Register(
            nameof(Class), typeof(ClassVM), typeof(ClassPropertyEditorControl));

        public ClassVM Class
        {
            get => GetValue(ClassProperty) as ClassVM;
            set => SetValue(ClassProperty, value);
        }

        public ClassPropertyEditorControl()
        {
            InitializeComponent();
        }
    }
}
