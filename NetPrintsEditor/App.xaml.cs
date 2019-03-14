using System.Windows;

namespace NetPrintsEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] StartupArguments
        {
            get;
            private set;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            StartupArguments = e.Args;
            base.OnStartup(e);
        }
    }
}
