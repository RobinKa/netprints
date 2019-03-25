using System;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    public partial class Method
    {
        [Obsolete]
        [OnDeserializing]
        private void FixVisibility(StreamingContext context)
        {
            // Set new visibility from old modifiers
            if (Modifiers.HasFlag(MethodModifiers.Public))
            {
                Visibility = MemberVisibility.Public;
            }
            else if (Modifiers.HasFlag(MethodModifiers.Protected))
            {
                Visibility = MemberVisibility.Protected;
            }
            else if (Modifiers.HasFlag(MethodModifiers.Private))
            {
                Visibility = MemberVisibility.Private;
            }
            else if (Modifiers.HasFlag(MethodModifiers.Internal))
            {
                Visibility = MemberVisibility.Internal;
            }
        }
    }
}
