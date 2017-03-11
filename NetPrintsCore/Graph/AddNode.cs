using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class AddNode : ExecNode
    {
        public AddNode()
        {
            AddInputDataPin("A", typeof(int));
            AddInputDataPin("B", typeof(int));
            AddOutputDataPin("X", typeof(int));
        }
    }
}
