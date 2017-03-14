using NetPrints.Core;
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

        public NodeInputDataPin TargetPin
        {
            get { return InputDataPins[0]; }
        }

        public IList<NodeInputDataPin> ArgumentPins
        {
            get { return InputDataPins.Skip(1).ToList(); }
        }

        public CallMethodNode(Method method, string methodName, IEnumerable<Type> inputTypes, IEnumerable<Type> outputTypes)
            : base(method)
        {
            MethodName = methodName;

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
