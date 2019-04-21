using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NetPrintsEditor.Commands;
using NetPrintsEditor.ViewModels;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for MemberVariableView.xaml
    /// </summary>
    public partial class MemberVariableView : UserControl
    {
        public MemberVariableView()
        {
            InitializeComponent();
        }

        public MemberVariableVM ViewModel
        {
            get => DataContext as MemberVariableVM;
        }

        private void OnRemoveVariableClicked(object sender, RoutedEventArgs e)
        {
            UndoRedoStack.Instance.DoCommand(NetPrintsCommands.RemoveVariable, DataContext);
        }

        private void OnVariableClicked(object sender, MouseButtonEventArgs e)
        {
            if (EditorCommands.SelectVariable.CanExecute(ViewModel))
            {
                EditorCommands.SelectVariable.Execute(ViewModel);
            }
        }

        private void OnAddGetterClicked(object sender, RoutedEventArgs e)
        {
            UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddGetter, DataContext);
        }

        private void OnAddSetterClicked(object sender, RoutedEventArgs e)
        {
            UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddSetter, DataContext);
        }

        private void OnRemoveGetterClicked(object sender, RoutedEventArgs e)
        {
            UndoRedoStack.Instance.DoCommand(NetPrintsCommands.RemoveGetter, DataContext);
        }

        private void OnRemoveSetterClicked(object sender, RoutedEventArgs e)
        {
            UndoRedoStack.Instance.DoCommand(NetPrintsCommands.RemoveSetter, DataContext);
        }

        private void OnGetterClicked(object sender, MouseButtonEventArgs e)
        {
            ViewModel.OpenGetterGraph();
        }

        private void OnSetterClicked(object sender, MouseButtonEventArgs e)
        {
            ViewModel.OpenSetterGraph();
        }

        private void OnMouseMoveTryDrag(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement element
                && element.DataContext != null)
            {
                DragDrop.DoDragDrop(element, element.DataContext, DragDropEffects.Copy);
            }
        }
    }
}
