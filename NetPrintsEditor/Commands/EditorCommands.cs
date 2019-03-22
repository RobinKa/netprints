using System.Windows.Input;

namespace NetPrintsEditor.Commands
{
    public static class EditorCommands
    {
        /// <summary>
        /// Command for opening the variables get / set dialog.
        /// </summary>
        public static readonly ICommand OpenVariableGetSet = new RoutedUICommand(nameof(OpenVariableGetSet), nameof(OpenVariableGetSet), typeof(EditorCommands));
    }
}
