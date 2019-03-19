using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Serialization;
using NetPrints.Translator;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Controls;
using NetPrintsEditor.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
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
    /// Interaction logic for ClassEditorWindow.xaml
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

        public ClassEditorWindow(ClassVM cls)
        {
            InitializeComponent();

            Class = cls;

            classViewer.Class = Class;
        }

        private void OnMethodDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is MethodVM m)
            {
                methodEditor.Method = m;
            }
        }

        private void OnMouseMoveTryDrag(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement element && 
                element.DataContext != null)
            {
                DragDrop.DoDragDrop(element, element.DataContext, DragDropEffects.Copy);
            }
        }

        private void OnCompileButtonClicked(object sender, RoutedEventArgs e)
        {
            Class.Project.CompileProject(false);
        }

        private void OnRunButtonClicked(object sender, RoutedEventArgs e)
        {
            ProjectVM project = Class.Project;

            project.PropertyChanged += OnProjectPropertyChangedWhileCompiling;
            project.CompileProject(true);
        }

        private void OnProjectPropertyChangedWhileCompiling(object sender, PropertyChangedEventArgs e)
        {
            ProjectVM project = Class.Project;

            if (e.PropertyName == nameof(project.IsCompiling) && !project.IsCompiling)
            {
                project.PropertyChanged -= OnProjectPropertyChangedWhileCompiling;

                if (project.LastCompilationSucceeded)
                {
                    project.RunProject();
                }
            }
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
            newMethod.ReturnNodes.First().PositionX = newMethod.EntryNode.PositionX + 400;
            newMethod.ReturnNodes.First().PositionY = newMethod.EntryNode.PositionY;
            GraphUtil.ConnectExecPins(newMethod.EntryNode.InitialExecutionPin, newMethod.ReturnNodes.First().ReturnPin);

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
            Class.Class.Attributes.Add(new Variable(e.Parameter as string, TypeSpecifier.FromType<object>()));
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
            
            e.CanExecute = e.Parameter is ConnectPinsParameters cp && 
                GraphUtil.CanConnectNodePins(cp.PinA.Pin, cp.PinB.Pin, 
                (a, b) => ProjectVM.Instance.ReflectionProvider.TypeSpecifierIsSubclassOf(a, b),
                (a, b) => ProjectVM.Instance.ReflectionProvider.HasImplicitCast(a, b));
        }

        private void CommandConnectPins_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ConnectPinsParameters cp = e.Parameter as ConnectPinsParameters;

            if (cp.PinA.Pin is NodeInputDataPin || cp.PinA.Pin is NodeOutputExecPin || cp.PinA.Pin is NodeInputTypePin)
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

            // Make sure the node will on the canvas
            if (p.PositionX < 0)
                p.PositionX = 0;

            if (p.PositionY < 0)
                p.PositionY = 0;

            object[] parameters = new object[] { p.Method }.Concat(p.ConstructorParameters).ToArray();
            Node node = Activator.CreateInstance(p.NodeType, parameters) as Node;
            node.PositionX = p.PositionX;
            node.PositionY = p.PositionY;

            methodEditor.grid.ContextMenu.IsOpen = false;
        }

        // Select Node

        private void CommandSelectNode_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is NodeVM;
        }

        private void CommandSelectNode_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            // Try to find the MethodVM corresponding to the passed NodeVM
            // and set its selected node

            NodeVM node = e.Parameter as NodeVM;
            MethodVM method = Class?.Methods.FirstOrDefault(m => m.Nodes.Contains(node));
            if(method != null)
            {
                method.SelectedNode = node;
            }
        }

        // Open Variable Get / Set

        private void CommandOpenVariableGetSet_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is VariableGetSetInfo;
        }

        private void CommandOpenVariableGetSet_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            methodEditor.ShowVariableGetSet((VariableGetSetInfo)e.Parameter);
        }

        // Change node overload
        private void CommandChangeNodeOverload_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is ChangeNodeOverloadParameters overloadParams &&
                overloadParams.Node != null && overloadParams.Node.CurrentOverload != null
                && overloadParams.Node.Overloads.Contains(overloadParams.NewOverload);
        }

        private void CommandChangeNodeOverload_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is ChangeNodeOverloadParameters overloadParams)
            {
                overloadParams.Node.ChangeOverload(overloadParams.NewOverload);
            }
            else
            {
                throw new ArgumentException("Expected type ChangeNodeOverloadParameters for e.Parameter.");
            }
        }

        #endregion

        #region Standard Commands
        private void CommandDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: Move logic into model / view model

            // Delete the currently selected node in the currently open method
            // Only delete the node if it is not an entry or the last return node
            if(methodEditor?.Method?.SelectedNode != null &&
                !(methodEditor.Method.SelectedNode.Node is EntryNode) &&
                !(methodEditor.Method.SelectedNode.Node is ReturnNode && methodEditor.Method.Method.ReturnNodes.Count() <= 1))
            {
                NodeVM deletedNode = methodEditor.Method.SelectedNode;
                methodEditor.Method.SelectedNode = null;
                
                // Remove the node from its method
                // This will trigger the correct events in MethodVM
                // so everything gets disconnected properly

                deletedNode.Method.Nodes.Remove(deletedNode.Node);
            }
        }

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

        private void OnAttributeClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is VariableVM v)
            {
                viewerTabControl.SelectedIndex = 1;
                variableViewer.Variable = v;
            }
        }

        private void OnRemoveAttributeClicked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is VariableVM v)
            {
                if(viewerTabControl.SelectedIndex == 1 && variableViewer.Variable == v)
                {
                    variableViewer.Variable = null;
                    viewerTabControl.SelectedIndex = 0;
                }

                Class.Class.Attributes.Remove(v.Variable);
            }
        }

        private void OnMethodClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is MethodVM m)
            {
                viewerTabControl.SelectedIndex = 2;
                methodViewer.Method = m;
            }
        }

        private void OnRemoveMethodClicked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is MethodVM m)
            {
                if (viewerTabControl.SelectedIndex == 2 && methodViewer.Method == m)
                {
                    methodViewer.Method = null;
                    viewerTabControl.SelectedIndex = 0;
                }

                if (methodEditor.Method == m)
                {
                    methodEditor.Method = null;
                }

                Class.Class.Methods.Remove(m.Method);
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
            // Save the entire project. If we only save the class
            // we could get issues like the project still referencing the
            // old class if the project isn't saved.
            ProjectVM.Instance.Save();
        }
    }
}
