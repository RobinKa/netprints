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
        public delegate void ItemSelectedHandler(object sender, string item);
        
        public static DependencyProperty ItemsProperty = DependencyProperty.Register(
            nameof(Items), typeof(IEnumerable), typeof(SearchableComboBox));

        private MethodInfoConverter methodInfoConverter;

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

            methodInfoConverter = new MethodInfoConverter();
            
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
            
            string itemText = methodInfoConverter.Convert(item, typeof(string), null, CultureInfo.CurrentUICulture) as string;

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
            if (searchList.SelectedItem != null && searchList.SelectedItem is MethodInfo methodInfo)
            {
                UndoRedoStack.Instance.DoCommand(NetPrintsCommands.AddNode, new NetPrintsCommands.AddNodeParameters
                (
                    typeof(CallStaticFunctionNode),
                    null,
                    0,
                    0,
                    methodInfo.DeclaringType.ToString(),
                    methodInfo.Name,
                    methodInfo.GetParameters().Select(p => p.ParameterType).ToArray(),
                    methodInfo.ReturnType == typeof(void) ? new Type[] { } : new Type[] { methodInfo.ReturnType }
                ));

                //CallStaticFunctionNode(Method method, string className, string methodName, 
                //    IEnumerable<Type> inputTypes, IEnumerable<Type> outputTypes)
                
            }
        }
    }
}
