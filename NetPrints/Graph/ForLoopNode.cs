using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
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

        public NodeInputExecPin ContinuePin
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
            AddInputExecPin("Exec");
            AddInputExecPin("Continue");
            
            AddOutputExecPin("Loop");
            AddOutputExecPin("Completed");

            AddInputDataPin("InitialIndex", TypeSpecifier.FromType<int>());
            AddInputDataPin("MaxIndex", TypeSpecifier.FromType<int>());

            AddOutputDataPin("Index", TypeSpecifier.FromType<int>());
        }
    }
}
