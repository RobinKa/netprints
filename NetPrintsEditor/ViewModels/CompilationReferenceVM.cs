using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using NetPrintsEditor.Compilation;

namespace NetPrintsEditor.ViewModels
{
    public class CompilationReferenceVM : INotifyPropertyChanged
    {
        public bool ShowIncludeInCompilationCheckBox
        {
            get => reference is SourceDirectoryReference;
        }

        public bool IncludeInCompilation
        {
            get => reference is SourceDirectoryReference sourceReference && sourceReference.IncludeInCompilation;
            set
            {
                if (reference is SourceDirectoryReference sourceDirectoryReference)
                {
                    sourceDirectoryReference.IncludeInCompilation = value;
                    OnPropertyChanged();
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public CompilationReference Reference
        {
            get => reference;
        }

        private readonly CompilationReference reference;

        public CompilationReferenceVM(CompilationReference compilationReference)
        {
            reference = compilationReference;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
