using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Converters;
using NetPrintsEditor.Dialogs;
using System;
using System.Collections;
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
            
            string itemText = suggestionConverter.Convert(item, typeof(string), null, CultureInfo.CurrentUICulture) as string;

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
                if(item.DataContext is MethodInfo methodInfo)
                {
                    if (methodInfo.IsStatic)
                    {
                        // CallStaticFunctionNode(Method method, TypeSpecifier classType, 
                        // string methodName, IEnumerable<TypeSpecifier> inputTypes, 
                        // IEnumerable<TypeSpecifier> outputTypes)

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(CallStaticFunctionNode),
                            null,
                            0,
                            0,

                            // Parameters
                            (TypeSpecifier)methodInfo.DeclaringType,
                            methodInfo.Name,
                            methodInfo.GetParameters().Select(p => (TypeSpecifier)p.ParameterType).ToArray(),
                            methodInfo.ReturnType == typeof(void) ? new TypeSpecifier[] { } : new TypeSpecifier[] { methodInfo.ReturnType }
                        ));
                    }
                    else
                    {
                        // CallMethodNode(Method method, string methodName, 
                        //     IEnumerable<TypeSpecifier> inputTypes, IEnumerable<TypeSpecifier> outputTypes)

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(CallMethodNode),
                            null,
                            0,
                            0,

                            // Parameters
                            (TypeSpecifier)methodInfo.DeclaringType,
                            methodInfo.Name,
                            methodInfo.GetParameters().Select(p => (TypeSpecifier)p.ParameterType).ToArray(),
                            methodInfo.ReturnType == typeof(void) ? new TypeSpecifier[] { } : new TypeSpecifier[] { methodInfo.ReturnType }
                        ));
                    }
                }
                else if(item.DataContext is PropertyInfo propertyInfo)
                {
                    // Open variable get / set for the property
                    // Determine whether the getters / setters are public via GetAccessors
                    // and the return type of the accessor methods

                    MethodInfo[] publicAccessors = propertyInfo.GetAccessors();
                    bool canGet = publicAccessors.Any(a => a.ReturnType != typeof(void));
                    bool canSet = publicAccessors.Any(a => a.ReturnType == typeof(void));
                    
                    VariableGetSetInfo variableInfo = new VariableGetSetInfo(
                        propertyInfo.Name, propertyInfo.PropertyType, 
                        canGet, canSet, 
                        propertyInfo.DeclaringType);

                    if (EditorCommands.OpenVariableGetSet.CanExecute(variableInfo))
                    {
                        EditorCommands.OpenVariableGetSet.Execute(variableInfo);
                    }
                }
                else if(item.DataContext is Type t)
                {
                    if(t == typeof(ForLoopNode))
                    {
                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(ForLoopNode),
                            null,
                            0,
                            0
                        ));
                    }
                    else if(t == typeof(IfElseNode))
                    {
                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(IfElseNode),
                            null,
                            0,
                            0
                        ));
                    }
                    else if(t == typeof(ConstructorNode))
                    {
                        SelectTypeDialog selectTypeDialog = new SelectTypeDialog();
                        if(selectTypeDialog.ShowDialog() == true)
                        {
                            Type selectedType = ReflectionUtil.GetTypeFromSpecifier(
                                selectTypeDialog.SelectedType);

                            if(selectedType == null)
                            {
                                throw new Exception($"Type {selectTypeDialog.SelectedType} was not found using reflection.");
                            }

                            // Get all public constructors for the type
                            ConstructorInfo[] constructors = selectedType.GetConstructors();

                            if (constructors.Length > 0)
                            {
                                // Just choose the first constructor we find
                                ConstructorInfo constructor = constructors[0];

                                // ConstructorNode(Method method, TypeSpecifier classType, 
                                //     IEnumerable<TypeSpecifier> argumentTypes)

                                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                                (
                                    typeof(ConstructorNode),
                                    null,
                                    0,
                                    0,

                                    // Parameters
                                    (TypeSpecifier)selectedType,
                                    constructor.GetParameters().Select(p => (TypeSpecifier)p.ParameterType).ToArray()
                                ));
                            }
                        }
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
