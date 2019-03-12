using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Core
{
    /// <summary>
    /// Graph for describing generic types.
    /// </summary>
    public class TypeGraph
    {
        /// <summary>
        /// Node that returns the final type.
        /// </summary>
        public TypeNode TypeReturnNode
        {
            get;
            private set;
        }

        public TypeSpecifier GetResultantType()
        {
            return GenericsHelper.DetermineTypeNodeType(TypeReturnNode);
        }

        public TypeGraph(TypeSpecifier outputType)
        {
            TypeReturnNode = new TypeNode(this, outputType);
        }
    }
}
