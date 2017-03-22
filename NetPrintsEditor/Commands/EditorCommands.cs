using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetPrintsEditor.Commands
{
    public static class EditorCommands
    {
        public static readonly ICommand OpenVariableGetSet = new RoutedUICommand(nameof(OpenVariableGetSet), nameof(OpenVariableGetSet), typeof(NetPrintsCommands));
    }
}
