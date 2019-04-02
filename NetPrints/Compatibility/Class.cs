using System;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    public partial class Class
    {
        [Obsolete]
        [OnDeserialized]
        private void FixVisibility(StreamingContext context)
        {
            // Set new visibility from old modifiers
            if (Modifiers.HasFlag(ClassModifiers.Public))
            {
                Modifiers &= ~(ClassModifiers.Public);
                Visibility = MemberVisibility.Public;
            }
            else if (Modifiers.HasFlag(ClassModifiers.Protected))
            {
                Modifiers &= ~(ClassModifiers.Protected);
                Visibility = MemberVisibility.Protected;
            }
            else if (Modifiers.HasFlag(ClassModifiers.Internal))
            {
                Modifiers &= ~(ClassModifiers.Internal);
                Visibility = MemberVisibility.Internal;
            }
            else if (Visibility == MemberVisibility.Invalid)
            {
                Visibility = MemberVisibility.Private;
            }
        }

        /// <summary>
        /// Attributes this class has.
        /// </summary>
        [DataMember]
        [Obsolete]
        public ObservableRangeCollection<OldVariable> Attributes { get; set; } = new ObservableRangeCollection<OldVariable>();
    }
}
