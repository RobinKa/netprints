using NetPrints.Core;

namespace NetPrintsEditor.Dialogs
{
    public class MakeDelegateTypeInfo
    {
        public TypeSpecifier Type
        {
            get;
        }

        public MakeDelegateTypeInfo(TypeSpecifier type)
        {
            Type = type;
        }
    }
}
