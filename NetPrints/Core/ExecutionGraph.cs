using NetPrints.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    [DataContract]
    public abstract class ExecutionGraph : NodeGraph
    {
        /// <summary>
        /// Entry node where execution starts.
        /// </summary>
        [DataMember]
        public ExecutionEntryNode EntryNode
        {
            get;
            protected set;
        }

        /// <summary>
        /// Ordered argument types this graph takes.
        /// </summary>
        public IEnumerable<BaseType> ArgumentTypes
        {
            get => EntryNode != null ? EntryNode.InputTypePins.Select(pin => pin.InferredType?.Value ?? TypeSpecifier.FromType<object>()).ToList() : new List<BaseType>();
        }

        /// <summary>
        /// Ordered argument types with their names this graph takes.
        /// </summary>
        public IEnumerable<Named<BaseType>> NamedArgumentTypes
        {
            get => EntryNode != null ? EntryNode.InputTypePins.Zip(EntryNode.OutputDataPins, (type, data) => (type, data))
                .Select(pair => new Named<BaseType>(pair.data.Name, pair.type.InferredType?.Value ?? TypeSpecifier.FromType<object>())).ToList() : new List<Named<BaseType>>();
        }

        /// <summary>
        /// Visibility of this graph.
        /// </summary>
        [DataMember]
        public MemberVisibility Visibility
        {
            get;
            set;
        } = MemberVisibility.Private;
    }
}
