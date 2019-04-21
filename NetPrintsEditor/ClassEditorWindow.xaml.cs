using MahApps.Metro.Controls;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Controls;
using NetPrintsEditor.Messages;
using NetPrintsEditor.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static NetPrintsEditor.Commands.NetPrintsCommands;

namespace NetPrintsEditor
{
    /// <summary>
    /// Interaction logic for ClassEditorWindow.xaml
    /// </summary>
    public partial class ClassEditorWindow : MetroWindow
    {
        public ClassEditorVM ViewModel
        {
            get => DataContext as ClassEditorVM;
            set => DataContext = value;
        }

        private readonly UndoRedoStack undoRedoStack = UndoRedoStack.Instance;

        public ClassEditorWindow()
        {
            InitializeComponent();
        }

        private void OnMethodDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is NodeGraphVM graphViewModel)
            {
                ViewModel.OpenGraph(graphViewModel.Graph);
            }
        }

        private void OnMouseMoveTryDrag(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement element
                && element.DataContext != null)
            {
                DragDrop.DoDragDrop(element, element.DataContext, DragDropEffects.Copy);
            }
        }

        private void OnCompileButtonClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.Project.CompileProject();
        }

        private void OnRunButtonClicked(object sender, RoutedEventArgs e)
        {
            Project project = ViewModel.Project;

            project.PropertyChanged += OnProjectPropertyChangedWhileCompiling;
            project.CompileProject();
        }

        private void OnProjectPropertyChangedWhileCompiling(object sender, PropertyChangedEventArgs e)
        {
            Project project = ViewModel.Project;

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
        // Select variable

        private void CommandSelectVariable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && e.Parameter is MemberVariableVM;
        }

        private void CommandSelectVariable_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is MemberVariableVM v)
            {
                viewerTabControl.SelectedIndex = 1;
                variableViewer.DataContext = v;
            }
        }

        // Remove Method

        private void CommandRemoveMethod_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && e.Parameter is string && ViewModel.Methods.Any(m => m.Name == e.Parameter as string);
        }

        private void CommandRemoveMethod_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.Methods.Remove(ViewModel.Methods.First(m => m.Name == e.Parameter as string));
        }

        // Add Variable

        private void CommandAddVariable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && e.Parameter is string && !ViewModel.Variables.Any(m => m.Name == e.Parameter as string);
        }

        private void CommandAddVariable_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.Class.Variables.Add(new Variable(ViewModel.Class, e.Parameter as string, TypeSpecifier.FromType<object>(), null, null, VariableModifiers.None));
        }

        // Remove Variable

        private void CommandRemoveVariable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && e.Parameter is MemberVariableVM variable && ViewModel.Variables.Any(v => v.Variable == variable.Variable);
        }

        private void CommandRemoveVariable_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            MemberVariableVM memberVariableVM = (MemberVariableVM)e.Parameter;

            if (viewerTabControl.SelectedIndex == 1 && variableViewer.DataContext == memberVariableVM)
            {
                variableViewer.DataContext = null;
                viewerTabControl.SelectedIndex = 0;
            }

            if (methodEditor.Graph?.Graph == memberVariableVM.Getter || methodEditor.Graph?.Graph == memberVariableVM.Setter)
            {
                methodEditor.Graph = null;
            }

            ViewModel.Class.Variables.Remove(memberVariableVM.Variable);
        }

        // Move node

        private void CommandSetNodePosition_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is SetNodePositionParameters p
                && FindNodeVMFromSetNodePositionParameters(p) != null;
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
            if (p.Node != null)
            {
                return p.Node;
            }

            // Find closed by name
            NodeVM node = ViewModel.Methods.FirstOrDefault(m => m.Name == p.Node.Method.Name)?.
                Nodes.FirstOrDefault(n => n.Name == p.Node.Name);

            return node;
        }

        // Connect pins

        private void CommandConnectPins_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ConnectPinsParameters xcp = e.Parameter as ConnectPinsParameters;

            e.CanExecute = e.Parameter is ConnectPinsParameters cp
                && GraphUtil.CanConnectNodePins(cp.PinA.Pin, cp.PinB.Pin,
                (a, b) => App.ReflectionProvider.TypeSpecifierIsSubclassOf(a, b),
                (a, b) => App.ReflectionProvider.HasImplicitCast(a, b));
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
            e.CanExecute = e.Parameter is AddNodeParameters p && (p.Graph != null || methodEditor.Graph != null);
        }

        private void CommandAddNode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddNodeParameters p = e.Parameter as AddNodeParameters;

            if (p.Graph == null)
            {
                p.Graph = methodEditor.Graph.Graph;
                Point mouseLoc = Mouse.GetPosition(methodEditor.methodEditorWindow.drawCanvas);
                p.PositionX = mouseLoc.X - mouseLoc.X % MethodEditorControl.GridCellSize;
                p.PositionY = mouseLoc.Y - mouseLoc.Y % MethodEditorControl.GridCellSize;
            }

            // Make sure the node will on the canvas
            if (p.PositionX < 0)
                p.PositionX = 0;

            if (p.PositionY < 0)
                p.PositionY = 0;

            object[] parameters = new object[] { p.Graph }.Concat(p.ConstructorParameters).ToArray();
            Node node = Activator.CreateInstance(p.NodeType, parameters) as Node;
            node.PositionX = p.PositionX;
            node.PositionY = p.PositionY;

            // If the node was created as part of a suggestion, connect it
            // to the relevant node pin.
            if (methodEditor?.SuggestionPin != null)
            {
                methodEditor.SuggestionPin.ConnectRelevant(node);
                methodEditor.SuggestionPin = null;
            }

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

            // TODO: Make selection nicer. Finding the corresponding method view model seems wrong since
            //       we have to search in all possible places that have methods.
            // TODO: Make this work with the class graph.

            NodeVM node = e.Parameter as NodeVM;
            NodeGraph graph = ViewModel?.Class?.Methods?.FirstOrDefault(m => m.Nodes.Contains(node.Node)) ??
                ViewModel?.Class?.Constructors?.FirstOrDefault(c => c.Nodes.Contains(node.Node)) ??
                ViewModel?.Class?.Variables?.FirstOrDefault(v => v.GetterMethod != null && v.GetterMethod.Nodes.Contains(node.Node))?.GetterMethod ??
                (NodeGraph)ViewModel?.Class?.Variables?.FirstOrDefault(v => v.SetterMethod != null && v.SetterMethod.Nodes.Contains(node.Node))?.SetterMethod;

            if (graph != null)
            {
                // TODO: graph.SelectedNodes = new[] { node };
            }
        }

        // Open Variable Get / Set

        private void CommandOpenVariableGetSet_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is VariableSpecifier;
        }

        private void CommandOpenVariableGetSet_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            methodEditor.ShowVariableGetSet((VariableSpecifier)e.Parameter);
        }

        // Change node overload
        private void CommandChangeNodeOverload_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is ChangeNodeOverloadParameters overloadParams
                && overloadParams.Node?.CurrentOverload != null
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

        // Add/Remove Getter/Setter
        private void CommandAddGetter_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is MemberVariableVM;
        }

        private void CommandAddGetter_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ((MemberVariableVM)e.Parameter).AddGetter();
        }

        private void CommandRemoveGetter_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is MemberVariableVM;
        }

        private void CommandRemoveGetter_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ((MemberVariableVM)e.Parameter).RemoveGetter();
        }

        private void CommandAddSetter_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is MemberVariableVM;
        }

        private void CommandAddSetter_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ((MemberVariableVM)e.Parameter).AddSetter();
        }

        private void CommandRemoveSetter_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is MemberVariableVM;
        }

        private void CommandRemoveSetter_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ((MemberVariableVM)e.Parameter).RemoveSetter();
        }

        #endregion

        #region Standard Commands
        private void CommandDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: Move logic into model / view model

            // Delete the currently selected node in the currently open method.
            // Only delete the node if it is not an entry or the main return node.

            if (methodEditor?.Graph?.SelectedNodes != null)
            {
                foreach (var selectedNode in methodEditor.Graph.SelectedNodes)
                {
                    if (!(selectedNode.Node is MethodEntryNode) && !(selectedNode.Node is ClassReturnNode)
                        && selectedNode.Node != (methodEditor.Graph.Graph as MethodGraph)?.MainReturnNode)
                    {
                        // Remove the node from its method
                        // This will trigger the correct events in MethodVM
                        // so everything gets disconnected properly

                        selectedNode.Graph.Nodes.Remove(selectedNode.Node);
                    }
                }

                // TODO: Use own VM instead of method editor graph vm
                methodEditor.Graph.DeselectNodes();
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
            string uniqueName = NetPrintsUtil.GetUniqueName("Method", ViewModel.Methods.Select(m => m.Name).ToList());
            ViewModel.CreateMethod(uniqueName, MethodEditorControl.GridCellSize);
        }

        // Add Constructor Button
        private void AddConstructorButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateConstructor(MethodEditorControl.GridCellSize);
        }

        // Add Variable Button
        private void AddVariableButton_Click(object sender, RoutedEventArgs e)
        {
            string uniqueName = NetPrintsUtil.GetUniqueName("Variable", ViewModel.Variables.Select(m => m.Name).ToList());
            undoRedoStack.DoCommand(NetPrintsCommands.AddVariable, uniqueName);
        }

        private void OverrideMethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var methodSpecifier = e.AddedItems[0] as MethodSpecifier;
                if (methodSpecifier != null)
                {
                    ViewModel.CreateOverrideMethod(methodSpecifier);
                }
            }

            overrideMethodBox.SelectedItem = null;
            overrideMethodBox.Text = "Override a method";
        }
        #endregion

        private void OnMethodClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is NodeGraphVM m)
            {
                viewerTabControl.SelectedIndex = 2;
                methodViewer.DataContext = m;
            }
        }

        private void OnRemoveMethodClicked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is NodeGraphVM m)
            {
                if (viewerTabControl.SelectedIndex == 2 && methodViewer.Graph?.Graph == m.Graph)
                {
                    methodViewer.DataContext = null;
                    viewerTabControl.SelectedIndex = 0;
                }

                if (methodEditor.Graph.Graph == m.Graph)
                {
                    methodEditor.Graph = null;
                }

                if (m.Graph is MethodGraph methodGraph && ViewModel.Class.Methods.Contains(methodGraph))
                {
                    ViewModel.Class.Methods.Remove(methodGraph);
                }
                else if (m.Graph is ConstructorGraph constructorGraph && ViewModel.Class.Constructors.Contains(constructorGraph))
                {
                    ViewModel.Class.Constructors.Remove(constructorGraph);
                }
            }
        }

        private void OnMethodEditorClicked(object sender, MouseButtonEventArgs e)
        {
            viewerTabControl.SelectedIndex = 0;
            classViewer.DataContext = ViewModel;
        }

        private void OnClassPropertiesClicked(object sender, RoutedEventArgs e)
        {
            viewerTabControl.SelectedIndex = 0;
            classViewer.DataContext = ViewModel;

            ViewModel.OpenClassGraph();
        }

        private void OnSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            // Save the entire project. If we only save the class
            // we could get issues like the project still referencing the
            // old class if the project isn't saved.
            ViewModel.Project.Save();
        }
    }
}
