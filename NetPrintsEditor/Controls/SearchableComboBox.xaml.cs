using NetPrints.Graph;
using NetPrintsEditor.Commands;
using NetPrintsEditor.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
                        //CallStaticFunctionNode(Method method, string className, string methodName, 
                        //    IEnumerable<Type> inputTypes, IEnumerable<Type> outputTypes)

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(CallStaticFunctionNode),
                            null,
                            0,
                            0,

                            // Parameters
                            methodInfo.DeclaringType.ToString(),
                            methodInfo.Name,
                            methodInfo.GetParameters().Select(p => p.ParameterType).ToArray(),
                            methodInfo.ReturnType == typeof(void) ? new Type[] { } : new Type[] { methodInfo.ReturnType }
                        ));
                    }
                    else
                    {
                        //CallMethodNode(Method method, string methodName, IEnumerable<Type> inputTypes, 
                        //    IEnumerable<Type> outputTypes)

                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(CallMethodNode),
                            null,
                            0,
                            0,

                            // Parameters
                            methodInfo.Name,
                            methodInfo.GetParameters().Select(p => p.ParameterType).ToArray(),
                            methodInfo.ReturnType == typeof(void) ? new Type[] { } : new Type[] { methodInfo.ReturnType }
                        ));
                    }
                } else if(item.DataContext is Type t)
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
                    } else if(t == typeof(IfElseNode))
                    {
                        UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                        (
                            typeof(IfElseNode),
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
