using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public event ItemSelectedHandler OnItemSelected;

        public static DependencyProperty ItemsProperty = DependencyProperty.Register(
            nameof(Items), typeof(IEnumerable), typeof(SearchableComboBox));

        public IEnumerable Items
        {
            get => searchList.ItemsSource;
            set
            {
                searchList.ItemsSource = value;
                if (CollectionViewSource.GetDefaultView(searchList.ItemsSource) is CollectionView view)
                {
                    view.Filter = Filter;
                }
            }
        }

        public SearchableComboBox()
        {
            InitializeComponent();
        }

        private bool Filter(object item)
        {
            if(string.IsNullOrEmpty(searchText.Text))
            {
                return true;
            }

            if (item is string s)
            {
                return s.IndexOf(searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
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
            OnItemSelected?.Invoke(sender, searchList.SelectedItem?.ToString());
        }
    }
}
