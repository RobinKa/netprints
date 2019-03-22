using NetPrints.Core;
using System.Windows;
using System.Windows.Controls;

namespace NetPrintsEditor.Controls
{
    public class VariableGetSetInfo
    {
        public TypeSpecifier TargetType
        {
            get;
        }

        public string Name
        {
            get;
        }

        public TypeSpecifier Type
        {
            get;
        }

        public object Tag
        {
            get;
            set;
        }

        public bool CanGet
        {
            get;
        }

        public bool CanSet
        {
            get;
        }

        public VariableModifiers Modifiers
        {
            get;
        }

        public VariableGetSetInfo(string name, TypeSpecifier type, bool canGet, bool canSet, VariableModifiers modifiers, TypeSpecifier targetType = null)
        {
            TargetType = targetType;
            Name = name;
            CanGet = canGet;
            CanSet = canSet;
            Type = type;
            Modifiers = modifiers;
        }
    }

    public delegate void VariableGetSetDelegate(VariableGetSetControl sender, 
        VariableGetSetInfo variableInfo, bool wasSet);

    /// <summary>
    /// Interaction logic for VariableGetSetControl.xaml
    /// </summary>
    public partial class VariableGetSetControl : UserControl
    {
        public static DependencyProperty VariableInfoProperty = DependencyProperty.Register(
            nameof(VariableInfo), typeof(VariableGetSetInfo), typeof(VariableGetSetControl));

        public event VariableGetSetDelegate OnVariableGetSet;

        public VariableGetSetInfo VariableInfo
        {
            get => (VariableGetSetInfo)GetValue(VariableInfoProperty);
            set => SetValue(VariableInfoProperty, value);
        }

        public VariableGetSetControl()
        {
            InitializeComponent();
        }

        private void OnVariableSetClicked(object sender, RoutedEventArgs e)
        {
            OnVariableGetSet?.Invoke(this, VariableInfo, true);
        }

        private void OnVariableGetClicked(object sender, RoutedEventArgs e)
        {
            OnVariableGetSet?.Invoke(this, VariableInfo, false);
        }
    }
}
