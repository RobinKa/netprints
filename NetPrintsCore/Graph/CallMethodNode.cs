using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class CallMethodNode : ExecNode
    {
        public string MethodName
        {
            get;
            private set;
        }

        public CallMethodNode(string methodName, IEnumerable<Type> inputTypes, IEnumerable<Type> outputTypes)
        {
            AddInputDataPin("Target", typeof(object));

            foreach(Type inputType in inputTypes)
            {
                AddInputDataPin(inputType.Name, inputType);
            }

            foreach(Type outputType in outputTypes)
            {
                AddOutputDataPin(outputType.Name, outputType);
            }
        }
    }
}
