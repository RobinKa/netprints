using System;
using GalaSoft.MvvmLight;
using NetPrints.Core;

namespace NetPrintsEditor.ViewModels
{
    public class CompilationReferenceVM : ViewModelBase
    {
        public bool ShowIncludeInCompilationCheckBox =>
            Reference is SourceDirectoryReference;

        public bool IncludeInCompilation
        {
            get => Reference is SourceDirectoryReference sourceReference && sourceReference.IncludeInCompilation;
            set
            {
                if (Reference is SourceDirectoryReference sourceDirectoryReference)
                {
                    sourceDirectoryReference.IncludeInCompilation = value;
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
    }
}
