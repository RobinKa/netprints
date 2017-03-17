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
using NetPrints.Translator;
using System.IO;
using System.CodeDom.Compiler;
using System.Threading;
using System.Diagnostics;

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
            cls.SuperType = typeof(object);
            cls.Namespace = "TestName.Space";
            cls.Name = "TestClass";
            Class = new ClassVM(cls);
            classViewer.Class = Class;
        }

        private void OnMethodListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            methodEditor.Method = methodList.SelectedItem as Method;
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
            Method newMethod = new Method(e.Parameter as string);
            newMethod.Class = Class.Class;
            newMethod.EntryNode.PositionX = 100;
            newMethod.EntryNode.PositionY = 100;
            newMethod.ReturnNode.PositionX = newMethod.EntryNode.PositionX + 400;
            newMethod.ReturnNode.PositionY = newMethod.EntryNode.PositionY;
            Class.Methods.Add(newMethod);
            //methodEditor.Method = newMethod;
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

        // Move node

        private void CommandSetNodePosition_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is SetNodePositionParameters p && FindNodeVMFromSetNodePositionParameters(p) != null;
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
            // Find open existing
            NodeVM nodeVM = methodEditor.NodeControls.FirstOrDefault(c => c.NodeVM.Node == p.Node.Node)?.NodeVM;

            // Find open by name
            if(nodeVM == null)
            {
                nodeVM = methodEditor.NodeControls.FirstOrDefault(c => c.NodeVM.Method.Name == p.Node.Method.Name && c.NodeVM.Name == p.Node.Name)?.NodeVM;
            }
            
            // Find closed by name
            if(nodeVM == null)
            {
                Node node = Class.Methods.FirstOrDefault(m => m.Name == p.Node.Method.Name)?.Nodes.FirstOrDefault(n => n.Name == p.Node.Name);
                if(node != null)
                {
                    nodeVM = new NodeVM(node);
                }
            }

            return nodeVM;
        }

        // Connect pins

        private void CommandConnectPins_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ConnectPinsParameters xcp = e.Parameter as ConnectPinsParameters;
            bool xcanConnect = GraphUtil.CanConnectNodePins(xcp.PinA, xcp.PinB);
            var xcA = FindPinControlFromPin(xcp.PinA);
            var xcB = FindPinControlFromPin(xcp.PinB);

            
            e.CanExecute = e.Parameter is ConnectPinsParameters cp && GraphUtil.CanConnectNodePins(cp.PinA, cp.PinB)
                && FindPinControlFromPin(cp.PinA) != null && FindPinControlFromPin(cp.PinB) != null;
        }

        private void CommandConnectPins_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ConnectPinsParameters cp = e.Parameter as ConnectPinsParameters;

            GraphUtil.ConnectNodePins(cp.PinA, cp.PinB);
        }

        private PinControl FindPinControlFromPin(NodePin pin)
        {
            foreach(NodeControl nodeControl in methodEditor.NodeControls)
            {
                PinControl pc = nodeControl.FindPinControl(pin);
                if(pc != null)
                {
                    return pc;
                }
            }

            return null;
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
                p.Method = methodEditor.Method;
                Point mouseLoc = Mouse.GetPosition(methodEditor.canvas);
                p.PositionX = mouseLoc.X;
                p.PositionY = mouseLoc.Y;
            }

            object[] parameters = new object[] { p.Method }.Union(p.ConstructorParameters).ToArray();
            Node node = Activator.CreateInstance(p.NodeType, parameters) as Node;
            node.PositionX = p.PositionX;
            node.PositionY = p.PositionY;

            methodEditor.CreateNodeControl(node);
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
            if (sender is ListViewItem item && item.DataContext is Variable v)
            {
                viewerTabControl.SelectedIndex = 1;
                variableViewer.Variable = v;
            }
        }

        private void OnMethodSelected(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.DataContext is Method m)
            {
                viewerTabControl.SelectedIndex = 2;
                methodViewer.Method = new MethodVM(m);
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
    }
}
