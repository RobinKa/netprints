using NetPrints.Core;
using NetPrintsEditor.Reflection;
using System.Collections.Generic;
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

        public static IReflectionProvider ReflectionProvider
        {
            get;
            private set;
        }

        public static ObservableRangeCollection<TypeSpecifier> NonStaticTypes
        {
            get;
        } = new ObservableRangeCollection<TypeSpecifier>();

        public static void ReloadReflectionProvider(IEnumerable<string> assemblyPaths, IEnumerable<string> sourcePaths, IEnumerable<string> sources)
        {
            ReflectionProvider = new MemoizedReflectionProvider(new ReflectionProvider(assemblyPaths, sourcePaths, sources));

            // Cache static types
            NonStaticTypes.ReplaceRange(ReflectionProvider.GetNonStaticTypes());
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            StartupArguments = e.Args;
            base.OnStartup(e);
        }
    }
}
