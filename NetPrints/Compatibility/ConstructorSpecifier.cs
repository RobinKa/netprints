using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    public partial class ConstructorSpecifier
    {
        [DataMember(Name = "_x003C_Arguments_x003E_k__BackingField", IsRequired = false, EmitDefaultValue = false)]
        private IList<TypeSpecifier> OldArguments
        {
            get => null;
            set
            {
                if (value != null)
                {
                    Arguments = value.Select(t => new Named<BaseType>(t.Name, t)).ToList();
                }
            }
        }

        [DataMember(Name = "_x003C_DeclaringType_x003E_k__BackingField", IsRequired = false, EmitDefaultValue = false)]
        private TypeSpecifier OldDeclaringType
        {
            get => null;
            set
            {
                if (!(value is null))
                {
                    DeclaringType = value;
                }
            }
        }
    }
}
