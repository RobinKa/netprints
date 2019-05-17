using NetPrints.Core;
using System.Collections.ObjectModel;

namespace NetPrints.Base
{
    public interface INodeOutputPin : INodePin
    {
        public IObservableCollectionView<INodeInputPin> OutgoingPins { get; }
    }
}
