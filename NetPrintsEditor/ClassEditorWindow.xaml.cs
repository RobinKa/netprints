using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using NetPrints.Core;
using NetPrintsEditor.ViewModels;
using NetPrintsEditor.Controls;
using NetPrintsEditor.Adorners;
using NetPrintsEditor.Commands;
using static NetPrintsEditor.Commands.NetPrintsCommands;

namespace NetPrintsEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ClassEditorWindow : Window
    {
        public ClassVM Class
        {
            get { return GetValue(ClassProperty) as ClassVM; }
            set { SetValue(ClassProperty, value); }
        }

        public static DependencyProperty ClassProperty = DependencyProperty.Register("Class", typeof(ClassVM), typeof(ClassEditorWindow));

        private UndoRedoStack undoRedoStack = UndoRedoStack.Instance;

        public ClassEditorWindow()
        {
            InitializeComponent();

            Class cls = new Class();
            Class = new ClassVM(cls);
        }

        private void OnMethodListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            methodEditor.Method = methodList.SelectedItem as Method;
        }

        #region Commands
        // Add Method

        private void CommandAddMethod_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Class != null && e.Parameter is string && !Class.Methods.Any(m => m.Name == e.Parameter as string);
        }

        private void CommandAddMethod_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Method newMethod = new Method(e.Parameter as string);
            newMethod.ArgumentTypes.Add(typeof(int));
            newMethod.ArgumentTypes.Add(typeof(int));
            newMethod.ReturnTypes.Add(typeof(string));
            newMethod.ReturnNode.PositionX = newMethod.EntryNode.PositionX + 200;
            Class.Methods.Add(newMethod);
            methodEditor.Method = newMethod;
        }

        // Remove Method

        private void CommandRemoveMethod_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Class != null && e.Parameter is string && Class.Methods.Any(m => m.Name == e.Parameter as string);
        }

        private void CommandRemoveMethod_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Class.Methods.Remove(Class.Methods.First(m => m.Name == e.Parameter as string));
        }

        // Add Attribute

        private void CommandAddAttribute_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Class != null && e.Parameter is string && !Class.Attributes.Any(m => m.Name == e.Parameter as string);
        }

        private void CommandAddAttribute_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Class.Attributes.Add(new Variable(e.Parameter as string, typeof(object)));
        }

        // Remove Attribute

        private void CommandRemoveAttribute_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Class != null && e.Parameter is string && Class.Attributes.Any(m => m.Name == e.Parameter as string);
        }

        private void CommandRemoveAttribute_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Class.Attributes.Remove(Class.Attributes.First(m => m.Name == e.Parameter as string));
        }

        #endregion

        #region Undo / Redo
        private void CommandUndo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            undoRedoStack.Undo();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            undoRedoStack.Redo();
        }
        #endregion

        #region Command Executors
        // Add Method Button
        private void AddMethodButton_Click(object sender, RoutedEventArgs e)
        {
            string uniqueName = NetPrintsUtil.GetUniqueName("Method", Class.Methods.Select(m => m.Name).ToList());
            undoRedoStack.DoCommand(NetPrintsCommands.AddMethod, uniqueName);
        }

        // Add Attribute Button
        private void AddAttributeButton_Click(object sender, RoutedEventArgs e)
        {
            string uniqueName = NetPrintsUtil.GetUniqueName("Variable", Class.Attributes.Select(m => m.Name).ToList());
            undoRedoStack.DoCommand(NetPrintsCommands.AddAttribute, uniqueName);
        }
        #endregion
    }
}
