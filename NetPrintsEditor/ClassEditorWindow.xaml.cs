using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Serialization;
using NetPrints.Translator;
using NetPrintsEditor.Commands;
using NetPrintsEditor.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private string previousStoragePath = null;

        public ClassEditorWindow(ClassVM cls)
        {
            InitializeComponent();

            Class = cls;

            if(File.Exists(Class.StoragePath))
            {
                previousStoragePath = Class.StoragePath;
            }

            classViewer.Class = Class;
        }

        private void OnMethodListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            methodEditor.Method = methodList.SelectedItem as MethodVM;
        }

        private void OnListItemMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Border b && b.DataContext != null)
            {
                DragDrop.DoDragDrop(b, b.DataContext, DragDropEffects.Copy);
            }
        }

        private void OnCompileButtonClick(object sender, RoutedEventArgs e)
        {
            compileButton.Content = "...";

            // Translate the class to C#
            ClassTranslator classTranslator = new ClassTranslator();
            string fullClassName = $"{Class.Namespace}.{Class.Name}";
            string code = classTranslator.TranslateClass(Class.Class);

            // Compile in another thread
            new Thread(() =>
            {
                if (!Directory.Exists("Compiled"))
                {
                    Directory.CreateDirectory("Compiled");
                }

                File.WriteAllText($"Compiled/{fullClassName}.txt", code);

                CompilerResults results = CompilerUtil.CompileStringToLibrary(code, $"Compiled/{fullClassName}.dll");

                File.WriteAllText($"Compiled/{fullClassName}_errors.txt", string.Join(Environment.NewLine, results.Errors.Cast<CompilerError>()));

                compileButton.Dispatcher.Invoke(() =>
                {
                    compileButton.Content = "Compile";
                });
            }).Start();
        }

        private void OnRunButtonClicked(object sender, RoutedEventArgs e)
        {
            runButton.Content = "...";

            // Translate the class to C#
            ClassTranslator classTranslator = new ClassTranslator();
            string fullClassName = $"{Class.Namespace}.{Class.Name}";
            string code = classTranslator.TranslateClass(Class.Class);

            // Compile in another thread
            new Thread(() =>
            {
                if (!Directory.Exists("Compiled"))
                {
                    Directory.CreateDirectory("Compiled");
                }

                File.WriteAllText($"Compiled/{fullClassName}.txt", code);

                CompilerResults results = CompilerUtil.CompileStringToExecutable(code, $"Compiled/{fullClassName}.exe");

                File.WriteAllText($"Compiled/{fullClassName}_errors.txt", string.Join(Environment.NewLine, results.Errors.Cast<CompilerError>()));
                
                runButton.Dispatcher.Invoke(() =>
                {
                    runButton.Content = "Run";
                });
                
                if (!results.Errors.HasErrors)
                {
                    Process.Start(System.IO.Path.Combine(Environment.CurrentDirectory, results.PathToAssembly));
                }
            }).Start();
        }

        #region Commands
        // Add Method

        private void CommandAddMethod_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Class != null && e.Parameter is string && !Class.Methods.Any(m => m.Name == e.Parameter as string);
        }

        private void CommandAddMethod_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Method newMethod = new Method(e.Parameter as string)
            {
                Class = Class.Class,
            };

            newMethod.EntryNode.PositionX = 100;
            newMethod.EntryNode.PositionY = 100;
            newMethod.ReturnNode.PositionX = newMethod.EntryNode.PositionX + 400;
            newMethod.ReturnNode.PositionY = newMethod.EntryNode.PositionY;
            GraphUtil.ConnectExecPins(newMethod.EntryNode.InitialExecutionPin, newMethod.ReturnNode.ReturnPin);

            Class.Class.Methods.Add(newMethod);
            methodEditor.Method = Class.Methods.Single(m => m.Method == newMethod);
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
            Class.Class.Attributes.Add(new Variable(e.Parameter as string, typeof(object)));
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

        // Move node

        private void CommandSetNodePosition_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is SetNodePositionParameters p && 
                FindNodeVMFromSetNodePositionParameters(p) != null;
        }

        private void CommandSetNodePosition_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SetNodePositionParameters p = e.Parameter as SetNodePositionParameters;
            NodeVM nodeVM = FindNodeVMFromSetNodePositionParameters(p);
            nodeVM.PositionX = p.NewPositionX;
            nodeVM.PositionY = p.NewPositionY;
        }

        public NodeVM FindNodeVMFromSetNodePositionParameters(SetNodePositionParameters p)
        {            
            if(p.Node != null)
            {
                return p.Node;
            }

            // Find closed by name
            NodeVM node = Class.Methods.FirstOrDefault(m => m.Name == p.Node.Method.Name)?.
                Nodes.FirstOrDefault(n => n.Name == p.Node.Name);

            return node;
        }

        // Connect pins

        private void CommandConnectPins_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ConnectPinsParameters xcp = e.Parameter as ConnectPinsParameters;
            
            e.CanExecute = e.Parameter is ConnectPinsParameters cp && GraphUtil.CanConnectNodePins(cp.PinA.Pin, cp.PinB.Pin);
        }

        private void CommandConnectPins_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ConnectPinsParameters cp = e.Parameter as ConnectPinsParameters;

            if (cp.PinA.Pin is NodeInputDataPin || cp.PinA.Pin is NodeOutputExecPin)
            {
                cp.PinA.ConnectedPin = cp.PinB;
            }
            else
            {
                cp.PinB.ConnectedPin = cp.PinA;
            }
        }

        // Add node

        private void CommandAddNode_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is AddNodeParameters p && (p.Method != null || methodEditor.Method != null);
        }

        private void CommandAddNode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddNodeParameters p = e.Parameter as AddNodeParameters;
            
            if (p.Method == null)
            {
                p.Method = methodEditor.Method.Method;
                Point mouseLoc = Mouse.GetPosition(methodEditor.methodEditorWindow);
                p.PositionX = mouseLoc.X;
                p.PositionY = mouseLoc.Y;
            }

            object[] parameters = new object[] { p.Method }.Union(p.ConstructorParameters).ToArray();
            Node node = Activator.CreateInstance(p.NodeType, parameters) as Node;
            node.PositionX = p.PositionX;
            node.PositionY = p.PositionY;

            methodEditor.grid.ContextMenu.IsOpen = false;
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

        private void OnVariableSelected(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.DataContext is VariableVM v)
            {
                viewerTabControl.SelectedIndex = 1;
                variableViewer.Variable = v;
            }
        }

        private void OnMethodSelected(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.DataContext is MethodVM m)
            {
                viewerTabControl.SelectedIndex = 2;
                methodViewer.Method = m;
            }
        }

        private void OnMethodEditorClicked(object sender, MouseButtonEventArgs e)
        {
            viewerTabControl.SelectedIndex = 0;
            classViewer.Class = Class;
        }

        private void OnClassPropertiesClicked(object sender, RoutedEventArgs e)
        {
            viewerTabControl.SelectedIndex = 0;
            classViewer.Class = Class;
        }

        private void OnSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            SerializationHelper.SaveClass(Class.Class, Class.StoragePath);

            // Delete old save file if different path and exists
            if(previousStoragePath != null && previousStoragePath != Class.StoragePath && File.Exists(previousStoragePath))
            {
                File.Delete(previousStoragePath);
            }

            previousStoragePath = Class.StoragePath;
        }
    }
}
