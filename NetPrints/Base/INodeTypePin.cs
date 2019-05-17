using NetPrints.Core;
using NetPrints.Graph;

namespace NetPrints.Base
{
    public interface INodeTypePin : INodePin
    {
        public ObservableValue<BaseType> InferredType { get; }
    }
}
