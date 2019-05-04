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
        /// Command for removing a method from a class.
        /// </summary>
        public static readonly RoutedUICommand RemoveMethod = new RoutedUICommand(nameof(RemoveMethod), nameof(RemoveMethod), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for adding an attribute to a class.
        /// </summary>
        public static readonly RoutedUICommand AddVariable = new RoutedUICommand(nameof(AddVariable), nameof(AddVariable), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for removing an attribute from a class.
        /// </summary>
        public static readonly RoutedUICommand RemoveVariable = new RoutedUICommand(nameof(RemoveVariable), nameof(RemoveVariable), typeof(NetPrintsCommands));

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
        /// Command for changing the overload of a node.
        /// </summary>
        public static readonly RoutedUICommand ChangeNodeOverload = new RoutedUICommand(nameof(ChangeNodeOverload), nameof(ChangeNodeOverload), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for adding a getter to a variable.
        /// </summary>
        public static readonly RoutedUICommand AddGetter = new RoutedUICommand(nameof(AddGetter), nameof(AddGetter), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for adding a setter to a variable.
        /// </summary>
        public static readonly RoutedUICommand AddSetter = new RoutedUICommand(nameof(AddSetter), nameof(AddSetter), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for removing a getter from a variable.
        /// </summary>
        public static readonly RoutedUICommand RemoveGetter = new RoutedUICommand(nameof(RemoveGetter), nameof(RemoveGetter), typeof(NetPrintsCommands));

        /// <summary>
        /// Command for removing a setter from a variable.
        /// </summary>
        public static readonly RoutedUICommand RemoveSetter = new RoutedUICommand(nameof(RemoveSetter), nameof(RemoveSetter), typeof(NetPrintsCommands));

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

        public class ConnectPinsParameters
        {
            public NodePinVM PinA;
            public NodePinVM PinB;
        }

        public class ChangeNodeOverloadParameters
        {
            public NodeVM Node;
            public object NewOverload;

            public ChangeNodeOverloadParameters(NodeVM node, object newOverload)
            {
                Node = node;
                NewOverload = newOverload;
            }
        }

        public delegate Tuple<ICommand, object> MakeUndoCommandDelegate(object parameters);

        public static Dictionary<ICommand, MakeUndoCommandDelegate> MakeUndoCommand = new Dictionary<ICommand, MakeUndoCommandDelegate>()
        {
            { RemoveMethod, (p) => new Tuple<ICommand, object>(DoNothing, p) },
            { AddVariable, (p) => new Tuple<ICommand, object>(RemoveVariable, p) },
            { RemoveVariable, (p) => new Tuple<ICommand, object>(AddVariable, p) },
            {
                SetNodePosition, (p) =>
                {
                    if (p is SetNodePositionParameters)
                    {
                        var np = p as SetNodePositionParameters;

                        SetNodePositionParameters undoParams = new SetNodePositionParameters(
                            np.Node, np.Node.Node.PositionX, np.Node.Node.PositionY);

                        return new Tuple<ICommand, object>(SetNodePosition, undoParams);
                    }

                    throw new ArgumentException("Expected parameters of type SetNodePositionParameters");
                }
            },
            { ConnectPins, (p) => new Tuple<ICommand, object>(DoNothing, p) },
            {
                ChangeNodeOverload, (p) =>
                {
                    if (p is ChangeNodeOverloadParameters overloadParams)
                    {
                        // Restore the old overload
                        var undoParams = new ChangeNodeOverloadParameters(overloadParams.Node, overloadParams.Node.CurrentOverload);
                        return new Tuple<ICommand, object>(ChangeNodeOverload, undoParams);
                    }

                    throw new ArgumentException("Expected parameters of type SetNodeOverloadParameters");
                }
            },
            { AddGetter, (p) => new Tuple<ICommand, object>(RemoveGetter, p) },
            { AddSetter, (p) => new Tuple<ICommand, object>(RemoveSetter, p) },
            { RemoveGetter, (p) => new Tuple<ICommand, object>(AddGetter, p) }, // TODO: Restore old method
            { RemoveSetter, (p) => new Tuple<ICommand, object>(AddSetter, p) }, // TODO: Restore old method
        };
    }
}
