using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Converters;
using NetPrintsEditor.Dialogs;
using NetPrintsEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NetPrintsEditor.Controls
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

    /// <summary>
    /// Interaction logic for SearchableComboBox.xaml
    /// </summary>
    public partial class SearchableComboBox : UserControl
    {
        //public delegate void ItemSelectedHandler(object sender, object item);

        public static DependencyProperty ItemsProperty = DependencyProperty.Register(
            nameof(Items), typeof(IEnumerable<SearchableComboBoxItem>), typeof(SearchableComboBox));

        private readonly SuggestionListConverter suggestionConverter;

        //public event ItemSelectedHandler OnItemSelected;

        public IEnumerable<SearchableComboBoxItem> Items
        {
            get => GetValue(ItemsProperty) as IEnumerable<SearchableComboBoxItem>;
            set
            {
                SetValue(ItemsProperty, value);

                if (searchList.Items.CanFilter)
                {
                    searchList.Items.Filter = Filter;
                }
            }
        }

        public SearchableComboBox()
        {
            InitializeComponent();

            suggestionConverter = new SuggestionListConverter();

            if (searchList.Items.CanFilter)
            {
                searchList.Items.Filter = Filter;
            }
        }

        private bool Filter(object item)
        {
            if (string.IsNullOrEmpty(searchText.Text))
            {
                return true;
            }

            object convertedItem = suggestionConverter.Convert(item, typeof(string), null, CultureInfo.CurrentUICulture);
            if (convertedItem is string listItemText)
            {
                return searchText.Text.Split(' ').All(searchTerm =>
                    listItemText.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            else
            {
                throw new Exception("Expected string type after conversion");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchList.ItemsSource != null)
            {
                CollectionViewSource.GetDefaultView(searchList.ItemsSource).Refresh();
            }
        }

        private void OnListItemSelected(object sender, MouseButtonEventArgs e)
        {
            /*if (sender is ListViewItem item)
            {
                OnItemSelected?.Invoke(item, data.Value);
            }*/

            if (sender is FrameworkElement element && element.DataContext is SearchableComboBoxItem data)
            {
                if (data.Value is MethodSpecifier methodSpecifier)
                {
                    UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                    (
                        typeof(CallMethodNode),
                        null,
                        0,
                        0,

                        // Parameters
                        methodSpecifier,
                        methodSpecifier.GenericArguments.Select(genArg => new GenericType(genArg.Name)).Cast<BaseType>().ToList()
                    ));
                }
                else if (data.Value is VariableSpecifier variableSpecifier)
                {
                    // Open variable get / set for the property
                    // Determine whether the getters / setters are public via GetAccessors
                    // and the return type of the accessor methods

                    if (EditorCommands.OpenVariableGetSet.CanExecute(variableSpecifier))
                    {
                        EditorCommands.OpenVariableGetSet.Execute(variableSpecifier);
                    }
                }
                else if (data.Value is MakeDelegateTypeInfo makeDelegateTypeInfo)
                {
                    var methods = ProjectVM.Instance.ReflectionProvider.GetMethods(
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

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(MakeDelegateNode),
                            null,
                            0,
                            0,

                            selectMethodDialog.SelectedMethod
                        ));
                    }
                }
                else if (data.Value is TypeSpecifier t)
                {
                    if (t == TypeSpecifier.FromType<ForLoopNode>())
                    {
                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(ForLoopNode),
                            null,
                            0,
                            0
                        ));
                    }
                    else if (t == TypeSpecifier.FromType<IfElseNode>())
                    {
                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(IfElseNode),
                            null,
                            0,
                            0
                        ));
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
                                ProjectVM.Instance.ReflectionProvider.GetConstructors(selectedType);

                            if (constructors?.Any() == true)
                            {
                                // Just choose the first constructor we find
                                ConstructorSpecifier constructorSpecifier = constructors.ElementAt(0);

                                // ConstructorNode(Method method, ConstructorSpecifier specifier)

                                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                                (
                                    typeof(ConstructorNode),
                                    null,
                                    0,
                                    0,

                                    // Parameters
                                    constructorSpecifier
                                ));
                            }
                        }
                    }
                    else if (t == TypeSpecifier.FromType<TypeOfNode>())
                    {
                        // TypeOfNode(Method method)

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(TypeOfNode),
                            null,
                            0,
                            0
                        ));
                    }
                    else if (t == TypeSpecifier.FromType<ExplicitCastNode>())
                    {
                        // ExplicitCastNode(Method method)

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(ExplicitCastNode),
                            null,
                            0,
                            0
                        ));
                    }
                    else if (t == TypeSpecifier.FromType<ReturnNode>())
                    {
                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(ReturnNode),
                            null,
                            0,
                            0
                        ));
                    }
                    else if (t == TypeSpecifier.FromType<MakeArrayNode>())
                    {
                        // MakeArrayNode(Method method)

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(MakeArrayNode),
                            null,
                            0,
                            0
                        ));
                    }
                    else if (t == TypeSpecifier.FromType<ThrowNode>())
                    {
                        // ThrowNode(Method method)

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(ThrowNode),
                            null,
                            0,
                            0
                        ));
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

                            UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                            (
                                typeof(LiteralNode),
                                null,
                                0,
                                0,

                                // Parameters
                                selectedType
                            ));
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

                            UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                            (
                                typeof(TypeNode),
                                null,
                                0,
                                0,

                                // Parameters
                                selectedType
                            ));
                        }
                    }
                    else if (t == TypeSpecifier.FromType<MakeArrayTypeNode>())
                    {
                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(MakeArrayTypeNode),
                            null,
                            0,
                            0
                        ));
                    }
                    else
                    {
                        // Build a type node
                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(TypeNode),
                            null,
                            0,
                            0,

                            // Parameters
                            t
                        ));
                    }
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Unselect
            searchList.SelectedItem = null;

            // Scroll to top left
            // https://stackoverflow.com/a/7182603/4332314
            if (VisualTreeHelper.GetChild(searchList, 0) is Decorator border)
            {
                var scrollViewer = border.Child as ScrollViewer;
                scrollViewer.ScrollToTop();
                scrollViewer.ScrollToLeftEnd();
            }

            // Clear search box and focus it
            searchText.Clear();
            searchText.Focus();
        }
    }
}
