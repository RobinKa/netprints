using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class CallStaticFunctionNode : ExecNode
    {
        public string MethodName
        {
            get;
            private set;
        }

        public string ClassName
        {
            get;
            private set;
        }

        public IList<NodeInputDataPin> ArgumentPins
        {
            get { return InputDataPins; }
        }

        public CallStaticFunctionNode(Method method, string className, string methodName, IEnumerable<Type> inputTypes, IEnumerable<Type> outputTypes)
            : base(method)
        {
            ClassName = className;
            MethodName = methodName;
            
            foreach(Type inputType in inputTypes)
            {
                AddInputDataPin(inputType.Name, inputType);
            }

            foreach(Type outputType in outputTypes)
            {
                AddOutputDataPin(outputType.Name, outputType);
            }
        }

        public override string ToString()
        {
            return $"Call Static {ClassName} {MethodName}";
        }
    }
}
