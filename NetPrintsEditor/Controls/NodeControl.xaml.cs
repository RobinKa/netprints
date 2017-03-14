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

        public PinControl FindPinControl(NodePin pin)
        {
            var a = inputDataPinList.ItemContainerGenerator.ContainerFromItem(pin);
            if(a == null)
            {
                a = outputDataPinList.ItemContainerGenerator.ContainerFromItem(pin);
                if(a == null)
                {
                    return null;
                }
            }

            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(a); i++)
            {
                var v = VisualTreeHelper.GetChild(a, i);
                if(v is PinControl pc && pc.Pin == pin)
                {
                    return pc;
                }
            }

            return null;
        }

        public NodeControl(NodeVM nodeVM)
        {
            this.nodeVM = nodeVM;
            
            InitializeComponent();

            inputExecPinList.ItemsSource = nodeVM.InputExecPins;
            outputExecPinList.ItemsSource = nodeVM.OutputExecPins;
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
    }
}
