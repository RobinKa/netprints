using NetPrints.Core;
using System.ComponentModel;

namespace NetPrintsEditor.ViewModels
{
    public class ReferenceListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Project Project
        {
            get;
            set;
        }

        public ReferenceListViewModel(Project project)
        {
            Project = project;
        }
    }
}
