using System.Windows.Input;

namespace NetPrintsEditor.Commands
{
    public static class EditorCommands
    {
        /// <summary>
        /// Command for opening the variables get / set dialog.
        /// </summary>
        public static readonly ICommand OpenVariableGetSet = new RoutedUICommand(nameof(OpenVariableGetSet), nameof(OpenVariableGetSet), typeof(EditorCommands));

        /// <summary>
        /// Command for opening a method graph.
        /// </summary>
        public static readonly ICommand OpenMethod = new RoutedUICommand(nameof(OpenMethod), nameof(OpenMethod), typeof(EditorCommands));

        /// <summary>
        /// Command for selecting a variable of a method.
        /// </summary>
        public static readonly ICommand SelectVariable = new RoutedUICommand(nameof(SelectVariable), nameof(SelectVariable), typeof(EditorCommands));
    }
}
