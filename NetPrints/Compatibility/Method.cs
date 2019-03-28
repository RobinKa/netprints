using System;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    public partial class Method
    {
        [Obsolete]
        private void FixVisibility(StreamingContext context)
        {
            // Set new visibility from old modifiers
            if (Modifiers.HasFlag(MethodModifiers.Public))
            {
                Modifiers &= ~(MethodModifiers.Public);
                Visibility = MemberVisibility.Public;
            }
            else if (Modifiers.HasFlag(MethodModifiers.Protected))
            {
                Modifiers &= ~(MethodModifiers.Protected);
                Visibility = MemberVisibility.Protected;
            }
            else if (Modifiers.HasFlag(MethodModifiers.Internal))
            {
                Modifiers &= ~(MethodModifiers.Internal);
                Visibility = MemberVisibility.Internal;
            }
            else if (Visibility == MemberVisibility.Invalid)
            {
                Visibility = MemberVisibility.Private;
            }
        }
    }
}
