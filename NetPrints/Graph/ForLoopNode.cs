using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing an integer-based for-loop.
    /// </summary>
    [DataContract]
    public class ForLoopNode : Node
    {
        /// <summary>
        /// Execution pin that gets executed for each loop.
        /// </summary>
        public NodeOutputExecPin LoopPin
        {
            get { return OutputExecPins[0]; }
        }
        
        /// <summary>
        /// Execution pin that gets executed when the loop is over.
        /// </summary>
        public NodeOutputExecPin CompletedPin
        {
            get { return OutputExecPins[1]; }
        }

        /// <summary>
        /// Input execution pin that executes the loop.
        /// </summary>
        public NodeInputExecPin ExecutionPin
        {
            get { return InputExecPins[0]; }
        }

        /// <summary>
        /// Input execution pin that skips the current loop step.
        /// Should only be executed from within this loop.
        /// </summary>
        public NodeInputExecPin ContinuePin
        {
            get { return InputExecPins[1]; }
        }

        /// <summary>
        /// Input data pin for the initial inclusive index value of the loop.
        /// </summary>
        public NodeInputDataPin InitialIndexPin
        {
            get { return InputDataPins[0]; }
        }

        /// <summary>
        /// Input data pin for the maximum exclusive index value of the loop.
        /// </summary>
        public NodeInputDataPin MaxIndexPin
        {
            get { return InputDataPins[1]; }
        }

        /// <summary>
        /// Output data pin for the current index value of the loop.
        /// Starts at the value of InitialIndexPin and increases up to,
        /// but not including, MaxIndexPin.
        /// </summary>
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

        public override string ToString()
        {
            return "For Loop";
        }
    }
}
