using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Converters;
using NetPrintsEditor.Dialogs;
using NetPrintsEditor.Reflection;
using NetPrintsEditor.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for SearchableComboBox.xaml
    /// </summary>
    public partial class SearchableComboBox : UserControl
    {
        //public delegate void ItemSelectedHandler(object sender, object item);
        
        public static DependencyProperty ItemsProperty = DependencyProperty.Register(
            nameof(Items), typeof(IEnumerable), typeof(SearchableComboBox));

        private SuggestionListConverter suggestionConverter;

        //public event ItemSelectedHandler OnItemSelected;

        public IEnumerable Items
        {
            get => GetValue(ItemsProperty) as IEnumerable;
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
            
            if(searchList.Items.CanFilter)
            {
                searchList.Items.Filter = Filter;
            }
        }

        private bool Filter(object item)
        {
            if(string.IsNullOrEmpty(searchText.Text))
            {
                return true;
            }

            string itemText = item.ToString();

            object convertedItem = suggestionConverter.Convert(item, typeof(string), null, CultureInfo.CurrentUICulture);
            if (convertedItem is SuggestionListItem listItem && listItem.Text is string listItemText)
            {
                itemText = listItemText;
            }

            return searchText.Text.Split(' ').All(searchTerm =>
                itemText.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
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
                OnItemSelected?.Invoke(item, item.DataContext);
            }*/

            if (sender is ListViewItem item)
            {
                if(item.DataContext is MethodSpecifier methodSpecifier)
                {
                    UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                    (
                        typeof(CallMethodNode),
                        null,
                        0,
                        0,

                        // Parameters
                        methodSpecifier,
                        Array.Empty<GenericType>()
                    ));
                }
                else if(item.DataContext is PropertySpecifier propertySpecifier)
                {
                    // Open variable get / set for the property
                    // Determine whether the getters / setters are public via GetAccessors
                    // and the return type of the accessor methods
                    // TODO: Get correct variable modifiers

                    VariableGetSetInfo variableInfo = new VariableGetSetInfo(
                        propertySpecifier.Name, propertySpecifier.Type, 
                        propertySpecifier.HasPublicGetter, propertySpecifier.HasPublicSetter,
                        VariableModifiers.Public, propertySpecifier.DeclaringType);

                    if (EditorCommands.OpenVariableGetSet.CanExecute(variableInfo))
                    {
                        EditorCommands.OpenVariableGetSet.Execute(variableInfo);
                    }
                }
                else if (item.DataContext is MakeDelegateTypeInfo makeDelegateTypeInfo)
                {
                    var instanceMethods = ProjectVM.Instance.ReflectionProvider.
                            GetPublicMethodsForType(makeDelegateTypeInfo.Type);

                    var staticMethods = ProjectVM.Instance.ReflectionProvider.
                            GetPublicStaticFunctionsForType(makeDelegateTypeInfo.Type);

                    SelectMethodDialog selectMethodDialog = new SelectMethodDialog()
                    {
                        Methods = instanceMethods.Concat(staticMethods),
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
                else if(item.DataContext is TypeSpecifier t)
                {
                    if(t == TypeSpecifier.FromType<ForLoopNode>())
                    {
                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(ForLoopNode),
                            null,
                            0,
                            0
                        ));
                    }
                    else if(t == TypeSpecifier.FromType<IfElseNode>())
                    {
                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(IfElseNode),
                            null,
                            0,
                            0
                        ));
                    }
                    else if(t == TypeSpecifier.FromType<ConstructorNode>())
                    {
                        SelectTypeDialog selectTypeDialog = new SelectTypeDialog();
                        if(selectTypeDialog.ShowDialog() == true)
                        {
                            TypeSpecifier selectedType = selectTypeDialog.SelectedType;

                            if(selectedType.Equals(null))
                            {
                                throw new Exception($"Type {selectTypeDialog.SelectedType} was not found using reflection.");
                            }

                            // Get all public constructors for the type
                            IEnumerable<ConstructorSpecifier> constructors = 
                                ProjectVM.Instance.ReflectionProvider.GetConstructors(selectedType);

                            if (constructors != null && constructors.Count() > 0)
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
                        SelectTypeDialog selectTypeDialog = new SelectTypeDialog();
                        if (selectTypeDialog.ShowDialog() == true)
                        {
                            TypeSpecifier selectedType = selectTypeDialog.SelectedType;

                            if (selectedType.Equals(null))
                            {
                                throw new Exception($"Type {selectTypeDialog.SelectedType} was not found using reflection.");
                            }

                            // TypeOfNode(Method method, TypeSpecifier type)

                            UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                            (
                                typeof(TypeOfNode),
                                null,
                                0,
                                0,

                                // Parameters
                                selectedType
                            ));
                        }
                    }
                    else if (t == TypeSpecifier.FromType<ExplicitCastNode>())
                    {
                        SelectTypeDialog selectTypeDialog = new SelectTypeDialog();
                        if (selectTypeDialog.ShowDialog() == true)
                        {
                            TypeSpecifier selectedType = selectTypeDialog.SelectedType;

                            if (selectedType.Equals(null))
                            {
                                throw new Exception($"Type {selectTypeDialog.SelectedType} was not found using reflection.");
                            }

                            // TypeOfNode(Method method, TypeSpecifier type)

                            UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                            (
                                typeof(ExplicitCastNode),
                                null,
                                0,
                                0,

                                // Parameters
                                selectedType
                            ));
                        }
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
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            searchText.Clear();
            searchText.Focus();
        }
    }
}
