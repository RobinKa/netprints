using MahApps.Metro.Controls;
using NetPrints.Core;
using System.Windows;

namespace NetPrintsEditor.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectTypeDialog.xaml
    /// </summary>
    public partial class SelectTypeDialog : MetroWindow
    {
        public static readonly DependencyProperty SelectedTypeProperty = DependencyProperty.Register(
            nameof(SelectedType), typeof(TypeSpecifier), typeof(SelectTypeDialog));

        public TypeSpecifier SelectedType
        {
            get => (TypeSpecifier)GetValue(SelectedTypeProperty);
            set => SetValue(SelectedTypeProperty, value);
        }

        public SelectTypeDialog()
        {
            InitializeComponent();

            SelectedType = TypeSpecifier.FromType<object>();
        }

        private void OnSelectButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
