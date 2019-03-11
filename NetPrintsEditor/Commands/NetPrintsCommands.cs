using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NetPrintsEditor.Commands
{
    /// <summary>
    /// Commands for modifying the NetPrints structures.
    /// </summary>
    public static class NetPrintsCommands
    {
        /// <summary>
        /// Command for adding a method to a class.
        /// </summary>
        public static readonly RoutedUICommand AddMethod = new RoutedUICommand(nameof(AddMethod), nameof(AddMethod), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for removing a method from a class.
        /// </summary>
        public static readonly RoutedUICommand RemoveMethod = new RoutedUICommand(nameof(RemoveMethod), nameof(RemoveMethod), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for adding an attribute to a class.
        /// </summary>
        public static readonly RoutedUICommand AddAttribute = new RoutedUICommand(nameof(AddAttribute), nameof(AddAttribute), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for removing an attribute from a class.
        /// </summary>
        public static readonly RoutedUICommand RemoveAttribute = new RoutedUICommand(nameof(RemoveAttribute), nameof(RemoveAttribute), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for setting the position of a node.
        /// </summary>
        public static readonly RoutedUICommand SetNodePosition = new RoutedUICommand(nameof(SetNodePosition), nameof(SetNodePosition), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for connecting two pins.
        /// </summary>
        public static readonly RoutedUICommand ConnectPins = new RoutedUICommand(nameof(ConnectPins), nameof(ConnectPins), typeof(NetPrintsCommands));

        /// <summary>
        /// Command that does nothing. Currently used in undoing when no undo is implemented.
        /// </summary>
        public static readonly RoutedUICommand DoNothing = new RoutedUICommand(nameof(DoNothing), nameof(DoNothing), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for adding a node to a method.
        /// </summary>
        public static readonly RoutedUICommand AddNode = new RoutedUICommand(nameof(AddNode), nameof(AddNode), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for selecting a node within a method.
        /// </summary>
        public static readonly RoutedUICommand SelectNode = new RoutedUICommand(nameof(SelectNode), nameof(SelectNode), typeof(NetPrintsCommands));

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
                    throw new ArgumentException($"Invalid parameters for constructor of {nodeType.FullName}");
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
            public NodePinVM PinA;
            public NodePinVM PinB;
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
            { SelectNode, (p) => new Tuple<ICommand, object>(DoNothing, p) },
        };
    }
}
