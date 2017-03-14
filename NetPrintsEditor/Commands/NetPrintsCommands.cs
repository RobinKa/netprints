using NetPrints.Graph;
using NetPrintsEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetPrintsEditor.Commands
{
    public static class NetPrintsCommands
    {
        public static readonly RoutedUICommand AddMethod = new RoutedUICommand(nameof(AddMethod), nameof(AddMethod), typeof(NetPrintsCommands));
        public static readonly RoutedUICommand RemoveMethod = new RoutedUICommand(nameof(RemoveMethod), nameof(RemoveMethod), typeof(NetPrintsCommands));
        public static readonly RoutedUICommand AddAttribute = new RoutedUICommand(nameof(AddAttribute), nameof(AddAttribute), typeof(NetPrintsCommands));
        public static readonly RoutedUICommand RemoveAttribute = new RoutedUICommand(nameof(RemoveAttribute), nameof(RemoveAttribute), typeof(NetPrintsCommands));
        public static readonly RoutedUICommand SetNodePosition = new RoutedUICommand(nameof(SetNodePosition), nameof(SetNodePosition), typeof(NetPrintsCommands));

        public class SetNodePositionParameters
        {
            public NodeVM Node;
            public double NewPositionX;
            public double NewPositionY;

            public SetNodePositionParameters(NodeVM node, double newPositionX, double newPositionY)
            {
                Node = node;
                NewPositionX = newPositionX;
                NewPositionY = newPositionY;
            }
        }

        public delegate Tuple<ICommand, object> MakeUndoCommandDelegate(object parameters);

        public static Dictionary<ICommand, MakeUndoCommandDelegate> MakeUndoCommand = new Dictionary<ICommand, MakeUndoCommandDelegate>()
        {
            { AddMethod, (p) => new Tuple<ICommand, object>(RemoveMethod, p) },
            { RemoveMethod, (p) => new Tuple<ICommand, object>(AddMethod, p) },
            { AddAttribute, (p) => new Tuple<ICommand, object>(RemoveAttribute, p) },
            { RemoveAttribute, (p) => new Tuple<ICommand, object>(AddAttribute, p) },
            {
                SetNodePosition, (p) =>
                {
                    if(p is SetNodePositionParameters)
                    {
                        var np = p as SetNodePositionParameters;

                        SetNodePositionParameters undoParams = new SetNodePositionParameters(
                            np.Node, np.Node.PositionX, np.Node.PositionY);

                        return new Tuple<ICommand, object>(SetNodePosition, undoParams);
                    }

                    return null;
                }
            }
        };
    }
}
