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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public VariableGetSetInfo(string name, TypeSpecifier type, bool canGet, bool canSet, TypeSpecifier targetType = null)
        {
            TargetType = targetType;
            Name = name;
            CanGet = canGet;
            CanSet = canSet;
            Type = type;
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
