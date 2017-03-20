using NetPrintsEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for VariableEditorControl.xaml
    /// </summary>
    public partial class VariableEditorControl : UserControl
    {
        public static DependencyProperty VariableProperty = DependencyProperty.Register(
            nameof(Variable), typeof(VariableVM), typeof(VariableEditorControl));

        public VariableVM Variable
        {
            get => (VariableVM)GetValue(VariableProperty);
            set => SetValue(VariableProperty, value);
        }

        public VariableEditorControl()
        {
            InitializeComponent();
        }
    }
}
