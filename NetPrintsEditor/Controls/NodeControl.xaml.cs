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
using System.ComponentModel;
using NetPrints.Graph;
using NetPrintsEditor.ViewModels;
using NetPrintsEditor.Commands;
using static NetPrintsEditor.Commands.NetPrintsCommands;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// Interaction logic for NodeControl.xaml
    /// </summary>
    public partial class NodeControl : UserControl
    {
        public NodeVM NodeVM
        {
            get => nodeVM;
        }

        private NodeVM nodeVM;

        public NodeControl(NodeVM nodeVM)
        {
            this.nodeVM = nodeVM;
            
            InitializeComponent();
            
            inputDataPinList.ItemsSource = nodeVM.InputDataPins;
            outputDataPinList.ItemsSource = nodeVM.OutputDataPins;

            nodeVM.PropertyChanged += OnNodePropertyChanged;

            RenderTransform = new TranslateTransform(nodeVM.PositionX, nodeVM.PositionY);
        }

        private void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(NodeVM.PositionX) || e.PropertyName == nameof(NodeVM.PositionY))
            {
                RenderTransform = new TranslateTransform(nodeVM.PositionX, nodeVM.PositionY);
            }
        }

        // Move node command
        private void CommandSetNodePosition_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is SetNodePositionParameters p && (p.Node == nodeVM || (p.MethodName == nodeVM.Method.Name && p.NodeName == nodeVM.Name));
        }

        private void CommandSetNodePosition_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SetNodePositionParameters p = e.Parameter as SetNodePositionParameters;
            if (p.Node != null)
            {
                p.Node.PositionX = p.NewPositionX;
                p.Node.PositionY = p.NewPositionY;
            }
        }
    }
}
