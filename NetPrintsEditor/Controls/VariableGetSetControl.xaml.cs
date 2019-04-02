using NetPrints.Core;
using System.Windows;
using System.Windows.Controls;

namespace NetPrintsEditor.Controls
{
    public delegate void VariableGetSetDelegate(VariableGetSetControl sender, 
        VariableSpecifier variableInfo, bool wasSet);

    /// <summary>
    /// Interaction logic for VariableGetSetControl.xaml
    /// </summary>
    public partial class VariableGetSetControl : UserControl
    {
        public static DependencyProperty VariableInfoProperty = DependencyProperty.Register(
            nameof(VariableSpecifier), typeof(VariableSpecifier), typeof(VariableGetSetControl));

        public event VariableGetSetDelegate OnVariableGetSet;

        public VariableSpecifier VariableSpecifier
        {
            get => (VariableSpecifier)GetValue(VariableInfoProperty);
            set => SetValue(VariableInfoProperty, value);
        }

        public VariableGetSetControl()
        {
            InitializeComponent();
        }

        private void OnVariableSetClicked(object sender, RoutedEventArgs e)
        {
            OnVariableGetSet?.Invoke(this, VariableSpecifier, true);
        }

        private void OnVariableGetClicked(object sender, RoutedEventArgs e)
        {
            OnVariableGetSet?.Invoke(this, VariableSpecifier, false);
        }
    }
}
