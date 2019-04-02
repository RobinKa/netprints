using NetPrints.Core;

namespace NetPrintsEditor.Dialogs
{
    public class MakeDelegateTypeInfo
    {
        public TypeSpecifier Type
        {
            get;
        }

        public TypeSpecifier FromType
        {
            get;
        }

        public MakeDelegateTypeInfo(TypeSpecifier type, TypeSpecifier fromType)
        {
            Type = type;
            FromType = fromType;
        }
    }
}
