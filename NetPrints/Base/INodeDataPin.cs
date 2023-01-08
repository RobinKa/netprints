using NetPrints.Core;
using NetPrints.Graph;

namespace NetPrints.Base
{
    public interface INodeDataPin : INodePin
    {
        public ObservableValue<BaseType> PinType { get; }
    }
}
