using GalaSoft.MvvmLight;
using NetPrints.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace NetPrintsEditor.ViewModels
{
    public class PinConnectionVM : ViewModelBase
    {
        public PinConnection Connection { get; }

        private static readonly Dictionary<Type, Brush> typeBrushes = new Dictionary<Type, Brush>()
        {
            [typeof(INodeExecutionPin)] = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xFF, 0xE0)),
            [typeof(INodeDataPin)] = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xE0, 0xFF)),
            [typeof(INodeTypePin)] = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xE0, 0xE0)),
        };

        public Brush Brush
        {
            get => typeBrushes.Single(x => (Connection.PinA.ConnectionType == NodePinConnectionType.Single ? Connection.PinA : Connection.PinB).GetType().GetInterfaces().Any(interf => interf == x.Key)).Value;
        }

        public PinConnectionVM(PinConnection connection)
        {
            Connection = connection;
        }
    }
}
