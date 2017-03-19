using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.ComponentModel;
using NetPrints.Graph;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using NetPrints.Core;

namespace NetPrintsEditor.ViewModels
{
    public class NodeVM : INotifyPropertyChanged
    {
        // Wrapped attributes of Node
        public string Name
        {
            get => node.Name;
            set
            {
                if (node.Name != value)
                {
                    node.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<NodeInputDataPin> InputDataPins { get => node.InputDataPins; }
        public ObservableCollection<NodeOutputDataPin> OutputDataPins { get => node.OutputDataPins; }
        public ObservableCollection<NodeInputExecPin> InputExecPins { get => node.InputExecPins; }
        public ObservableCollection<NodeOutputExecPin> OutputExecPins { get => node.OutputExecPins; }
        public bool IsPure { get => node.IsPure; }

        // Wrapped Node
        public Node Node
        {
            get => node;
            set
            {
                if (node != value)
                {
                    node = value;
                    OnPropertyChanged();
                }
            }
        }

        public Method Method
        {
            get => node.Method;
        }

        public double PositionX
        {
            get => node.PositionX;
            set
            {
                if (node.PositionX != value)
                {
                    node.PositionX = value;
                    OnPropertyChanged();
                }
            }
        }

        public double PositionY
        {
            get => node.PositionY;
            set
            {
                if (node.PositionY != value)
                {
                    node.PositionY = value;
                    OnPropertyChanged();
                }
            }
        }

        private Node node;

        public NodeVM(Node node)
        {
            this.node = node;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
