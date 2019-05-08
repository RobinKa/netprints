using GalaSoft.MvvmLight;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Converters;
using NetPrintsEditor.Dialogs;
using NetPrintsEditor.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetPrintsEditor.ViewModels
{
    public class SearchableComboBoxItem
    {
        public string Category { get; set; }
        public object Value { get; set; }

        public SearchableComboBoxItem(string c, object v)
        {
            Category = c;
            Value = v;
        }
    }

    public class SuggestionListVM : ViewModelBase
    {
        private readonly SuggestionListConverter suggestionConverter = new SuggestionListConverter();

        public IEnumerable<SearchableComboBoxItem> Items { get; set; }
        public NodeGraph Graph { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public NodePin SuggestionPin { get; set; }
        public Action HideContextMenu { get; set; }

        public string SearchText { get; set; } = "";
        private string[] splitSearchText = new string[0];

        public event EventHandler ItemsChanged;

        public SuggestionListVM()
        {

        }

        public void OnItemsChanged() => ItemsChanged?.Invoke(this, EventArgs.Empty);

        public bool ItemFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return true;
            }

            object convertedItem = suggestionConverter.Convert(item, typeof(string), null, CultureInfo.CurrentUICulture);
            if (convertedItem is string listItemText)
            {
                return splitSearchText.All(searchTerm =>
                    listItemText.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            else
            {
                throw new Exception("Expected string type after conversion");
            }
        }

        public void OnSearchTextChanged() => splitSearchText = SearchText.Split(' ');

        private void AddNode<T>(params object[] arguments)
        {
            var msg = new AddNodeMessage
            (
                typeof(T),
                Graph,
                PositionX,
                PositionY,
                SuggestionPin,

                // Parameters
                arguments
            );

            MessengerInstance.Send(msg);

            HideContextMenu?.Invoke();
        }

        public void OnItemSelected(object selectedValue)
        {
            // TODO: Move dialogs to view
            if (selectedValue is MethodSpecifier methodSpecifier)
            {
                AddNode<CallMethodNode>(methodSpecifier, methodSpecifier.GenericArguments.Select(genArg => new GenericType(genArg.Name)).Cast<BaseType>().ToList());
            }
            else if (selectedValue is VariableSpecifier variableSpecifier)
            {
                // Open variable get / set for the property
                // Determine whether the getters / setters are public via GetAccessors
                // and the return type of the accessor methods

                if (EditorCommands.OpenVariableGetSet.CanExecute(variableSpecifier))
                {
                    EditorCommands.OpenVariableGetSet.Execute(variableSpecifier);
                }
            }
            else if (selectedValue is MakeDelegateTypeInfo makeDelegateTypeInfo)
            {
                var methods = App.ReflectionProvider.GetMethods(
                    new Reflection.ReflectionProviderMethodQuery()
                    .WithType(makeDelegateTypeInfo.Type)
                    .WithVisibleFrom(makeDelegateTypeInfo.FromType));

                SelectMethodDialog selectMethodDialog = new SelectMethodDialog()
                {
                    Methods = methods,
                };

                if (selectMethodDialog.ShowDialog() == true)
                {
                    // MakeDelegateNode(Method method, MethodSpecifier methodSpecifier)

                    AddNode<MakeDelegateNode>(selectMethodDialog.SelectedMethod);
                }
            }
            else if (selectedValue is TypeSpecifier t)
            {
                if (t == TypeSpecifier.FromType<ForLoopNode>())
                {
                    AddNode<ForLoopNode>();
                }
                else if (t == TypeSpecifier.FromType<IfElseNode>())
                {
                    AddNode<IfElseNode>();
                }
                else if (t == TypeSpecifier.FromType<ConstructorNode>())
                {
                    SelectTypeDialog selectTypeDialog = new SelectTypeDialog();
                    if (selectTypeDialog.ShowDialog() == true)
                    {
                        TypeSpecifier selectedType = selectTypeDialog.SelectedType;

                        if (selectedType.Equals(null))
                        {
                            throw new Exception($"Type {selectTypeDialog.SelectedType} was not found using reflection.");
                        }

                        // Get all public constructors for the type
                        IEnumerable<ConstructorSpecifier> constructors =
                            App.ReflectionProvider.GetConstructors(selectedType);

                        if (constructors?.Any() == true)
                        {
                            // Just choose the first constructor we find
                            ConstructorSpecifier constructorSpecifier = constructors.ElementAt(0);

                            // ConstructorNode(Method method, ConstructorSpecifier specifier)

                            AddNode<ConstructorNode>(constructorSpecifier);
                        }
                    }
                }
                else if (t == TypeSpecifier.FromType<TypeOfNode>())
                {
                    // TypeOfNode(Method method)
                    AddNode<TypeOfNode>();
                }
                else if (t == TypeSpecifier.FromType<ExplicitCastNode>())
                {
                    // ExplicitCastNode(Method method)
                    AddNode<ExplicitCastNode>();
                }
                else if (t == TypeSpecifier.FromType<ReturnNode>())
                {
                    AddNode<ReturnNode>();
                }
                else if (t == TypeSpecifier.FromType<MakeArrayNode>())
                {
                    // MakeArrayNode(Method method)
                    AddNode<MakeArrayNode>();
                }
                else if (t == TypeSpecifier.FromType<ThrowNode>())
                {
                    // ThrowNode(Method method)
                    AddNode<ThrowNode>();
                }
                else if (t == TypeSpecifier.FromType<TernaryNode>())
                {
                    // TernaryNode(NodeGraph graph)
                    AddNode<TernaryNode>();
                }
                else if (t == TypeSpecifier.FromType<LiteralNode>())
                {
                    SelectTypeDialog selectTypeDialog = new SelectTypeDialog();
                    if (selectTypeDialog.ShowDialog() == true)
                    {
                        TypeSpecifier selectedType = selectTypeDialog.SelectedType;

                        if (selectedType.Equals(null))
                        {
                            throw new Exception($"Type {selectTypeDialog.SelectedType} was not found using reflection.");
                        }

                        // LiteralNode(Method method, TypeSpecifier literalType)
                        AddNode<LiteralNode>(selectedType);
                    }
                }
                else if (t == TypeSpecifier.FromType<TypeNode>())
                {
                    SelectTypeDialog selectTypeDialog = new SelectTypeDialog();
                    if (selectTypeDialog.ShowDialog() == true)
                    {
                        TypeSpecifier selectedType = selectTypeDialog.SelectedType;

                        if (selectedType.Equals(null))
                        {
                            throw new Exception($"Type {selectTypeDialog.SelectedType} was not found using reflection.");
                        }

                        // LiteralNode(Method method, TypeSpecifier literalType)
                        AddNode<TypeNode>(selectedType);
                    }
                }
                else if (t == TypeSpecifier.FromType<MakeArrayTypeNode>())
                {
                    AddNode<MakeArrayTypeNode>();
                }
                else if (t == TypeSpecifier.FromType<AwaitNode>())
                {
                    AddNode<AwaitNode>();
                }
                else if (t == TypeSpecifier.FromType<DefaultNode>())
                {
                    AddNode<DefaultNode>();
                }
                else
                {
                    // Build a type node
                    AddNode<TypeNode>(t);
                }
            }
        }
    }
}
