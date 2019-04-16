using System;
using System.Linq;
using System.Runtime.Serialization;
using NetPrints.Graph;

namespace NetPrints.Core
{
    public partial class Class
    {
        [Obsolete]
        [OnDeserialized]
        private void FixCompatibility(StreamingContext context)
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

            // Fix old variables in VariableNode. Called from here to make sure class
            // is initialized for finding its new variables.
            foreach (var varNode in Methods.SelectMany(m => m.Nodes).OfType<VariableNode>())
            {
                varNode.FixOldVariable();
            }

            // Fix old saves not having constructors
            if (Constructors is null)
            {
                Constructors = new ObservableRangeCollection<Method>();
            }
        }

        /// <summary>
        /// Attributes this class has.
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        [Obsolete]
        public ObservableRangeCollection<OldVariable> Attributes
        {
            get => null;
            set
            {
                Variables = new ObservableRangeCollection<Variable>(value.Select(oldVar => new Variable(this, oldVar.Name, oldVar.VariableType, null, null, oldVar.Modifiers) { Visibility = oldVar.Visibility }));
            }
        }
    }
}
