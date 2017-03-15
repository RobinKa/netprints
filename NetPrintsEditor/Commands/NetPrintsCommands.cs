using NetPrints.Core;
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
        public static readonly RoutedUICommand ConnectPins = new RoutedUICommand(nameof(ConnectPins), nameof(ConnectPins), typeof(NetPrintsCommands));
        public static readonly RoutedUICommand DoNothing = new RoutedUICommand(nameof(DoNothing), nameof(DoNothing), typeof(NetPrintsCommands));
        public static readonly RoutedUICommand AddNode = new RoutedUICommand(nameof(AddNode), nameof(AddNode), typeof(NetPrintsCommands));
        
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

        public class AddNodeParameters
        {
            public Type NodeType;
            public Method Method;
            public double PositionX;
            public double PositionY;
            public object[] ConstructorParameters;

            public AddNodeParameters(Type nodeType, Method method, double posX, double posY, params object[] constructorParameters)
            {
                if(!nodeType.IsSubclassOf(typeof(Node)) || nodeType.IsAbstract)
                {
                    throw new ArgumentException("Invalid type for node");
                }

                Type[] constructorParamTypes = (new Type[] { typeof(Method) }).Concat
                    (constructorParameters.Select(p => p.GetType()))
                    .ToArray();

                if (nodeType.GetConstructor(constructorParamTypes) == null)
                {
                    throw new ArgumentException("Invalid parameters for constructor of node");
                }

                NodeType = nodeType;
                Method = method;
                PositionX = posX;
                PositionY = posY;
                ConstructorParameters = constructorParameters;
            }
        }

        public class ConnectPinsParameters
        {
            public NodePin PinA;
            public NodePin PinB;
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
            },
            { ConnectPins, (p) => new Tuple<ICommand, object>(DoNothing, p) },
            { AddNode, (p) => new Tuple<ICommand, object>(DoNothing, p) },
        };
    }
}
