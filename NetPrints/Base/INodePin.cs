using NetPrints.Core;

namespace NetPrints.Base
{
    public enum NodePinConnectionType
    {
        Single,
        Multiple
    }

    /// <summary>
    /// Interface for node pins.
    /// </summary>
    public interface INodePin
    {
        /// <summary>
        /// Name of the pin.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Node this pin is contained in.
        /// </summary>
        public INode Node { get; }

        public NodePinConnectionType ConnectionType { get; }
        public ObservableRangeCollection<INodePin> ConnectedPins { get; }
    }
}
