using MahApps.Metro.Controls;
using NetPrints.Core;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NetPrintsEditor.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectMethodDialog.xaml
    /// </summary>
    public partial class SelectMethodDialog : MetroWindow
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
