using System;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    public partial class Variable
    {
        [Obsolete]
        [OnDeserialized]
        private void FixVisibility(StreamingContext context)
        {
            // Set new visibility from old modifiers
            if (Modifiers.HasFlag(VariableModifiers.Public))
            {
                Modifiers &= ~(VariableModifiers.Public);
                Visibility = MemberVisibility.Public;
            }
            else if (Modifiers.HasFlag(VariableModifiers.Protected))
            {
                Modifiers &= ~(VariableModifiers.Protected);
                Visibility = MemberVisibility.Protected;
            }
            else if (Modifiers.HasFlag(VariableModifiers.Internal))
            {
                Modifiers &= ~(VariableModifiers.Internal);
                Visibility = MemberVisibility.Internal;
            }
            else if (Visibility == MemberVisibility.Invalid)
            {
                Visibility = MemberVisibility.Private;
            }
        }
    }
}
