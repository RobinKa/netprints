using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    /// <summary>
    /// Method type. Contains common things usually associated with methods such as its arguments and its name.
    /// </summary>
    [DataContract]
    public class ConstructorGraph : ExecutionGraph
    {
        /// <summary>
        /// Creates a method given its name.
        /// </summary>
        public ConstructorGraph()
        {
            EntryNode = new ConstructorEntryNode(this);
        }

        public override string ToString()
        {
            return Class.Name;
        }
    }
}
