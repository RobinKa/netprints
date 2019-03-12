using System;
using System.Collections.Generic;
using System.Text;
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
    /// Interaction logic for SuggestionListItem.xaml
    /// </summary>
    public partial class SuggestionListItem : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(object), typeof(SuggestionListItem));

        public static readonly DependencyProperty IconPathProperty = DependencyProperty.Register(
            nameof(IconPath), typeof(string), typeof(SuggestionListItem));

        public object Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
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
    }
}
