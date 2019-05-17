using NetPrints.Core;
using System.Collections.ObjectModel;

namespace NetPrints.Base
{
    public interface INodeInputPin : INodePin
    {
        public IObservableCollectionView<INodeOutputPin> IncomingPins { get; }
    }
}
