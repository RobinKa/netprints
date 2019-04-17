using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NetPrints.Core;
using PropertyChanged;

namespace NetPrintsEditor.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class CompilationReferenceVM
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
