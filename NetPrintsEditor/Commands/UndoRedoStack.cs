using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace NetPrintsEditor.Commands
{
    /// <summary>
    /// Stack that keeps track of commands and supports redoing and undoing them.
    /// </summary>
    public class UndoRedoStack
    {
        public static UndoRedoStack Instance { get; } = new UndoRedoStack();

        private UndoRedoStack() { }

        private struct DoUndoPair
        {
            public ICommand DoCommand;
            public object DoParameters;

            public ICommand UndoCommand;
            public object UndoParameters;
        }

        private readonly Stack<DoUndoPair> undoStack = new Stack<DoUndoPair>();
        private readonly Stack<DoUndoPair> redoStack = new Stack<DoUndoPair>();

        /// <summary>
        /// Executes a command and adds it to the stack.
        /// </summary>
        /// <param name="doCommand">Command to execute.</param>
        /// <param name="doParameters">Parameters for the command.</param>
        public void DoCommand(ICommand doCommand, object doParameters)
        {
            Tuple<ICommand, object> undoPair = NetPrintsCommands.MakeUndoCommand[doCommand](doParameters);

            undoStack.Push(new DoUndoPair()
            {
                DoCommand = doCommand,
                DoParameters = doParameters,
                UndoCommand = undoPair.Item1,
                UndoParameters = undoPair.Item2,
            });

            redoStack.Clear();

            if (doCommand.CanExecute(doParameters))
            {
                doCommand.Execute(doParameters);
            }
        }

        /// <summary>
        /// Undoes the last command.
        /// </summary>
        /// <returns>Whether a command was undone.</returns>
        public bool Undo()
        {
            if (undoStack.Count > 0)
            {
                DoUndoPair pair = undoStack.Peek();

                if (pair.UndoCommand.CanExecute(pair.UndoParameters))
                {
                    undoStack.Pop();
                    redoStack.Push(pair);
                    pair.UndoCommand.Execute(pair.UndoParameters);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Redoes the last undone command.
        /// </summary>
        /// <returns>Whether a command was redone.</returns>
        public bool Redo()
        {
            if (redoStack.Count > 0)
            {
                DoUndoPair pair = redoStack.Peek();

                if (pair.DoCommand.CanExecute(pair.DoParameters))
                {
                    redoStack.Pop();
                    undoStack.Push(pair);
                    pair.DoCommand.Execute(pair.DoParameters);

                    return true;
                }
            }

            return false;
        }
    }
}
