using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class ReturnNode : Node
    {
        public ReturnNode(IEnumerable<Type> returnTypes)
        {
            AddInputExecPin("Exec");

            foreach (Type returnType in returnTypes)
            {
                AddInputDataPin(returnType.Name, returnType);
            }
        }
    }
}
