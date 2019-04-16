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
        public NodeGraphVM Graph
        {
            get => DataContext as NodeGraphVM;
        }

        public MethodPropertyEditorControl()
        {
            InitializeComponent();
        }
    }
}
