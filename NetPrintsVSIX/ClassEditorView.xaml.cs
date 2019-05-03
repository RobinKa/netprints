using MahApps.Metro.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Serialization;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Controls;
using NetPrintsEditor.Messages;
using NetPrintsEditor.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static NetPrintsEditor.Commands.NetPrintsCommands;

namespace NetPrints.VSIX
{
    /// <summary>
    /// Interaction logic for ClassEditorWindow.xaml
    /// </summary>
    public partial class ClassEditorView : UserControl, IVsPersistDocData2, IPersistFileFormat
    {
        public ClassEditorVM ViewModel
        {
            get => DataContext as ClassEditorVM;
            set => DataContext = value;
        }

        private readonly UndoRedoStack undoRedoStack = UndoRedoStack.Instance;

        public ClassEditorView()
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

            if (graphEditor.Graph?.Graph == memberVariableVM.Getter || graphEditor.Graph?.Graph == memberVariableVM.Setter)
            {
                graphEditor.Graph = null;
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
            nodeVM.Node.PositionX = p.NewPositionX;
            nodeVM.Node.PositionY = p.NewPositionY;
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
                (a, b) => NetPrintsEditor.App.ReflectionProvider.TypeSpecifierIsSubclassOf(a, b),
                (a, b) => NetPrintsEditor.App.ReflectionProvider.HasImplicitCast(a, b));
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
            e.CanExecute = e.Parameter is AddNodeParameters p && (p.Graph != null || graphEditor.Graph != null);
        }

        private void CommandAddNode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddNodeParameters p = e.Parameter as AddNodeParameters;

            if (p.Graph == null)
            {
                p.Graph = graphEditor.Graph.Graph;
                Point mouseLoc = Mouse.GetPosition(graphEditor.DrawCanvas);
                p.PositionX = mouseLoc.X - mouseLoc.X % GraphEditorView.GridCellSize;
                p.PositionY = mouseLoc.Y - mouseLoc.Y % GraphEditorView.GridCellSize;
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
            if (graphEditor?.Graph?.SuggestionPin != null)
            {
                GraphUtil.ConnectRelevantPins(graphEditor.Graph.SuggestionPin,
                    node, NetPrintsEditor.App.ReflectionProvider.TypeSpecifierIsSubclassOf,
                    NetPrintsEditor.App.ReflectionProvider.HasImplicitCast);

                graphEditor.Graph.SuggestionPin = null;
            }

            graphEditor.Grid.ContextMenu.IsOpen = false;
        }

        // Open Variable Get / Set

        private void CommandOpenVariableGetSet_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is VariableSpecifier;
        }

        private void CommandOpenVariableGetSet_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            graphEditor.ShowVariableGetSet((VariableSpecifier)e.Parameter);
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

            if (graphEditor?.Graph?.SelectedNodes != null)
            {
                foreach (var selectedNode in graphEditor.Graph.SelectedNodes)
                {
                    if (!(selectedNode.Node is MethodEntryNode) && !(selectedNode.Node is ClassReturnNode)
                        && selectedNode.Node != (graphEditor.Graph.Graph as MethodGraph)?.MainReturnNode)
                    {
                        // Remove the node from its method
                        // This will trigger the correct events in MethodVM
                        // so everything gets disconnected properly

                        selectedNode.Graph.Nodes.Remove(selectedNode.Node);
                    }
                }

                // TODO: Use own VM instead of method editor graph vm
                graphEditor.Graph.DeselectNodes();
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
            ViewModel.CreateMethod(uniqueName, GraphEditorView.GridCellSize);
        }

        // Add Constructor Button
        private void AddConstructorButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateConstructor(GraphEditorView.GridCellSize);
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

                if (graphEditor.Graph?.Graph == m.Graph)
                {
                    graphEditor.Graph = null;
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

        #region Persistence
        // TODO: Move these out of the view / DI the implementation

        /// <summary>
        /// Path where the class is stored.
        /// </summary>
        private string classPath;

        private bool dirty;
        private bool initialGeneratedCodeChanged;

        public int GetGuidEditorType(out Guid pClassID)
        {
            throw new NotImplementedException();
        }

        public int IsDocDataDirty(out int pfDirty)
        {
            pfDirty = dirty ? 1 : 0;
            return VSConstants.S_OK;
        }

        public int SetUntitledDocPath(string pszDocDataPath)
        {
            throw new NotImplementedException();
        }

        public int LoadDocData(string pszMkDocument)
        {
            classPath = pszMkDocument;

            ViewModel.PropertyChanged += OnViewModelPropertyChanged;

            return VSConstants.S_OK;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.GeneratedCode))
            {
                if (initialGeneratedCodeChanged)
                {
                    dirty = true;
                }
                else
                {
                    initialGeneratedCodeChanged = true;
                }
            }
        }

        public int SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            SerializationHelper.SaveClass(ViewModel.Class, classPath);

            // Generate C# code
            // TODO: Move this out of view
            CompilationUtil.CompileNetPrintsClass(classPath, Path.ChangeExtension(classPath, ".cs"));
            dirty = false;

            pbstrMkDocumentNew = classPath;
            pfSaveCanceled = 0;

            return VSConstants.S_OK;
        }

        public int Close()
        {
            return VSConstants.S_OK;
        }

        public int OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
        {
            return VSConstants.S_OK;
        }

        public int RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            classPath = pszMkDocumentNew;
            // TODO: Delete old files?

            return VSConstants.S_OK;
        }

        public int IsDocDataReloadable(out int pfReloadable)
        {
            pfReloadable = 0;

            return VSConstants.S_OK;
        }

        public int ReloadDocData(uint grfFlags)
        {
            return VSConstants.S_OK;
        }

        public int SetDocDataDirty(int fDirty)
        {
            return VSConstants.S_OK;
        }

        public int IsDocDataReadOnly(out int pfReadOnly)
        {
            pfReadOnly = 0;

            return VSConstants.S_OK;
        }

        public int SetDocDataReadOnly(int fReadOnly)
        {
            return VSConstants.S_OK;
        }

        public int GetClassID(out Guid pClassID)
        {
            pClassID = Guid.Empty;
            return VSConstants.S_OK;
        }

        public int IsDirty(out int pfIsDirty)
        {
            pfIsDirty = dirty ? 1 : 0;
            return VSConstants.S_OK;
        }

        public int InitNew(uint nFormatIndex)
        {
            return VSConstants.S_OK;
        }

        public int Load(string pszFilename, uint grfMode, int fReadOnly)
        {
            return VSConstants.S_OK;
        }

        public int Save(string pszFilename, int fRemember, uint nFormatIndex)
        {
            return VSConstants.S_OK;
        }

        public int SaveCompleted(string pszFilename)
        {
            return VSConstants.S_OK;
        }

        public int GetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            ppszFilename = classPath;
            pnFormatIndex = 0;

            return VSConstants.S_OK;
        }

        public int GetFormatList(out string ppszFormatList)
        {
            ppszFormatList = null;

            return VSConstants.S_OK;
        }
        #endregion
    }
}
