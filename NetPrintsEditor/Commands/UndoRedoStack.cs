using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Input;

namespace NetPrintsEditor.Commands
{
    public class UndoRedoStack
    {
        public static UndoRedoStack Instance
        {
            get => instance;
        }

        private static UndoRedoStack instance = new UndoRedoStack();

        private UndoRedoStack() { }

        private struct DoUndoPair
        {
            public ICommand DoCommand;
            public object DoParameters;

            public ICommand UndoCommand;
            public object UndoParameters;
        }

        private Stack<DoUndoPair> undoStack = new Stack<DoUndoPair>();
        private Stack<DoUndoPair> redoStack = new Stack<DoUndoPair>();

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

        public bool Undo()
        {
            if(undoStack.Count > 0)
            {
                DoUndoPair pair = undoStack.Peek();
            
                if(pair.UndoCommand.CanExecute(pair.UndoParameters))
                {
                    undoStack.Pop();
                    redoStack.Push(pair);
                    pair.UndoCommand.Execute(pair.UndoParameters);

                    return true;
                }
            }

            return false;
        }

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
