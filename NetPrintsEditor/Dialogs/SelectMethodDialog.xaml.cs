using NetPrints.Core;
using System;
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
using System.Windows.Shapes;

namespace NetPrintsEditor.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectMethodDialog.xaml
    /// </summary>
    public partial class SelectMethodDialog : Window
    {
        public static readonly DependencyProperty SelectedMethodProperty = DependencyProperty.Register(
            nameof(SelectedMethod), typeof(MethodSpecifier), typeof(SelectMethodDialog));

        public MethodSpecifier SelectedMethod
        {
            get => (MethodSpecifier)GetValue(SelectedMethodProperty);
            set => SetValue(SelectedMethodProperty, value);
        }

        public static readonly DependencyProperty MethodsProperty = DependencyProperty.Register(
            nameof(Methods), typeof(IEnumerable<MethodSpecifier>), typeof(SelectMethodDialog));

        public IEnumerable<MethodSpecifier> Methods
        {
            get => (IEnumerable<MethodSpecifier>)GetValue(MethodsProperty);
            set => SetValue(MethodsProperty, value);
        }

        public SelectMethodDialog()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if(e.Property == MethodsProperty)
            {
                SelectedMethod = Methods.FirstOrDefault();
            }
        }

        private void OnSelectButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
