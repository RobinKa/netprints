using System.Windows;
using System.Windows.Controls;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for SuggestionListItem.xaml
    /// </summary>
    public partial class SuggestionListItem : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(object), typeof(SuggestionListItem));

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            nameof(Category), typeof(string), typeof(SuggestionListItem));

        public static readonly DependencyProperty IconPathProperty = DependencyProperty.Register(
            nameof(IconPath), typeof(string), typeof(SuggestionListItem));

        public object Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Category
        {
            get => (string)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        public string IconPath
        {
            get => (string)GetValue(IconPathProperty);
            set => SetValue(IconPathProperty, value);
        }

        public SuggestionListItem()
        {
            InitializeComponent();
        }

        public override string ToString()
        {
            return Text.ToString();
        }
    }
}
