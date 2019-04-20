using GalaSoft.MvvmLight;
using NetPrints.Core;

namespace NetPrintsEditor.ViewModels
{
    public class ReferenceListViewModel : ViewModelBase
    {
        public Project Project
        {
            get; set;
        }

        public ReferenceListViewModel(Project project)
        {
            Project = project;
        }
    }
}
