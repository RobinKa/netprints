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
            get => Reference is SourceDirectoryReference;
        }

        public bool IncludeInCompilation
        {
            get => Reference is SourceDirectoryReference sourceReference && sourceReference.IncludeInCompilation;
            set
            {
                if (Reference is SourceDirectoryReference sourceDirectoryReference)
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

        public CompilationReference Reference { get; }

        public CompilationReferenceVM(CompilationReference compilationReference)
        {
            Reference = compilationReference;
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
