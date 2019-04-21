using GalaSoft.MvvmLight;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Translator;
using NetPrintsEditor.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Threading;

namespace NetPrintsEditor.ViewModels
{
    public class ClassEditorVM : ViewModelBase
    {
        public Project Project => Class?.Project;

        public NodeGraphVM OpenedGraph { get; set; }

        // Wrapped attributes of Class
        public ObservableViewModelCollection<MemberVariableVM, Variable> Variables { get; set; }

        public ObservableViewModelCollection<NodeGraphVM, MethodGraph> Methods { get; set; }

        public ObservableViewModelCollection<NodeGraphVM, ConstructorGraph> Constructors { get; set; }

        /// <summary>
        /// Specifiers for methods that this class can override.
        /// </summary>
        public IEnumerable<MethodSpecifier> OverridableMethods =>
            Class.AllBaseTypes.SelectMany(type => App.ReflectionProvider.GetOverridableMethodsForType(type));

        public TypeSpecifier Type => Class?.Type;

        public string FullName => Class?.FullName;

        public string Namespace
        {
            get => Class.Namespace;
            set => Class.Namespace = value;
        }

        public string Name
        {
            get => Class?.Name;
            set => Class.Name = value;
        }

        public ClassModifiers Modifiers
        {
            get => Class?.Modifiers ?? ClassModifiers.None;
            set => Class.Modifiers = value;
        }

        public MemberVisibility Visibility
        {
            get => Class?.Visibility ?? MemberVisibility.Invalid;
            set => Class.Visibility = value;
        }

        public IEnumerable<MemberVisibility> PossibleVisibilities => new[]
        {
            MemberVisibility.Internal,
            MemberVisibility.Private,
            MemberVisibility.Protected,
            MemberVisibility.Public,
        };

        public ClassGraph Class { get; set; }

        /// <summary>
        /// Generated code for the current class.
        /// </summary>
        public string GeneratedCode { get; set; }

        private readonly ClassTranslator classTranslator = new ClassTranslator();

        private readonly Timer codeTimer;

        public ClassEditorVM(ClassGraph cls)
        {
            MessengerInstance.Register<OpenGraphMessage>(this, OnOpenGraphReceived);

            Class = cls;

            codeTimer = new Timer(1000);
            codeTimer.Elapsed += (sender, eventArgs) =>
            {
                codeTimer.Stop();

                string code;

                try
                {
                    code = classTranslator.TranslateClass(Class);
                }
                catch (Exception ex)
                {
                    code = ex.ToString();
                }

                Dispatcher.CurrentDispatcher.Invoke(() => GeneratedCode = code);

                codeTimer.Start();
            };
            codeTimer.Start();
        }

        ~ClassEditorVM()
        {
            codeTimer?.Stop();
        }

        private void OnClassChanged()
        {
            Methods = new ObservableViewModelCollection<NodeGraphVM, MethodGraph>(
                Class.Methods, m => new NodeGraphVM(m) { Class = this } );

            Constructors = new ObservableViewModelCollection<NodeGraphVM, ConstructorGraph>(
                Class.Constructors, c => new NodeGraphVM(c) { Class = this });

            Variables = new ObservableViewModelCollection<MemberVariableVM, Variable>(
                Class.Variables, v => new MemberVariableVM(v));
        }

        private void OnOpenGraphReceived(OpenGraphMessage msg)
        {
            OpenedGraph = new NodeGraphVM(msg.Graph);
        }

        public void OpenGraph(NodeGraph graph)
        {
            MessengerInstance.Send(new OpenGraphMessage(graph));
        }

        public void OpenClassGraph()
        {
            MessengerInstance.Send(new OpenGraphMessage(Class));
        }

        public void CreateConstructor(double gridCellSize)
        {
            var newConstructor = new ConstructorGraph()
            {
                Class = Class,
                Visibility = MemberVisibility.Public,
            };

            newConstructor.EntryNode.PositionX = gridCellSize * 4;
            newConstructor.EntryNode.PositionY = gridCellSize * 4;

            Class.Constructors.Add(newConstructor);

            OpenGraph(newConstructor);
        }

        public void CreateMethod(string name, double gridCellSize)
        {
            var newMethod = new MethodGraph(name)
            {
                Class = Class,
            };

            newMethod.EntryNode.PositionX = gridCellSize * 4;
            newMethod.EntryNode.PositionY = gridCellSize * 4;
            newMethod.ReturnNodes.First().PositionX = newMethod.EntryNode.PositionX + gridCellSize * 15;
            newMethod.ReturnNodes.First().PositionY = newMethod.EntryNode.PositionY;
            GraphUtil.ConnectExecPins(newMethod.EntryNode.InitialExecutionPin, newMethod.MainReturnNode.ReturnPin);

            Class.Methods.Add(newMethod);

            OpenGraph(newMethod);
        }

        public void CreateOverrideMethod(MethodSpecifier methodSpecifier)
        {
            MethodGraph methodGraph = GraphUtil.AddOverrideMethod(Class, methodSpecifier);

            if (methodGraph != null)
            {
                OpenGraph(methodGraph);
            }
        }
    }
}
