using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class ForLoopNode : Node
    {
        public NodeOutputExecPin LoopPin
        {
            get { return OutputExecPins[0]; }
        }
        
        public NodeOutputExecPin CompletedPin
        {
            get { return OutputExecPins[1]; }
        }

        public NodeInputExecPin ExecutionPin
        {
            get { return InputExecPins[0]; }
        }

        public NodeInputExecPin NextLoopPin
        {
            get { return InputExecPins[1]; }
        }

        public NodeInputDataPin InitialIndexPin
        {
            get { return InputDataPins[0]; }
        }

        public NodeInputDataPin MaxIndexPin
        {
            get { return InputDataPins[1]; }
        }

        public NodeOutputDataPin IndexPin
        {
            get { return OutputDataPins[0]; }
        }

        public ForLoopNode(Method method)
            : base(method)
        {
            AddInputExecPin("StartLoop");
            AddInputExecPin("NextLoop");
            
            AddOutputExecPin("ExecLoop");
            AddOutputExecPin("ExecCompleted");

            AddInputDataPin("InitialIndex", typeof(int));
            AddInputDataPin("MaxIndex", typeof(int));

            AddOutputDataPin("Index", typeof(int));
        }
    }
}
