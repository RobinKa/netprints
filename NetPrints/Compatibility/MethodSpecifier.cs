using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    public partial class MethodSpecifier
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

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [Obsolete("Use Parameters instead.")]
        private IList<Named<BaseType>> Arguments
        {
            get => null;
            set => Parameters = value.Select(arg => new MethodParameter(arg.Name, arg.Value, MethodParameterPassType.Default)).ToList();
        }
    }
}
