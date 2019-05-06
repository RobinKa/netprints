using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    /// <summary>
    /// Type graph that returns a type.
    /// </summary>
    [DataContract]
    public class TypeGraph : NodeGraph
    {
        /// <summary>
        /// Return node of this type graph that receives the type.
        /// </summary>
        public TypeReturnNode ReturnNode
        {
            get => Nodes.OfType<TypeReturnNode>().Single();
        }

        /// <summary>
        /// TypeSpecifier for the type this graph returns.
        /// </summary>
        public TypeSpecifier ReturnType
        {
            get => (TypeSpecifier)ReturnNode.TypePin.InferredType?.Value ?? TypeSpecifier.FromType<object>();
        }

        public TypeGraph()
        {
            _ = new TypeReturnNode(this);
        }
    }
}
