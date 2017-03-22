using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Reflection
{
    [Serializable]
    public class PropertySpecifier
    {
        public string Name
        {
            get;
        }
        
        public TypeSpecifier DeclaringType
        {
            get;
        }
        
        public TypeSpecifier Type
        {
            get;
        }
        
        public bool HasPublicGetter
        {
            get;
        }

        public bool HasPublicSetter
        {
            get;
        }
        

        public PropertySpecifier(string name, TypeSpecifier type, bool hasPublicGetter, 
            bool hasPublicSetter, TypeSpecifier declaringType)
        {
            Name = name;
            Type = type;
            HasPublicGetter = hasPublicGetter;
            HasPublicSetter = HasPublicSetter;
            DeclaringType = declaringType;
        }

        public static implicit operator PropertySpecifier(PropertyInfo propertyInfo)
        {
            MethodInfo[] publicAccessors = propertyInfo.GetAccessors();
            bool hasPublicGetter = publicAccessors.Any(a => a.ReturnType != typeof(void));
            bool hasPublicSetter = publicAccessors.Any(a => a.ReturnType == typeof(void));

            return new PropertySpecifier(
                propertyInfo.Name,
                propertyInfo.PropertyType,
                hasPublicGetter,
                hasPublicSetter,
                propertyInfo.DeclaringType);
        }
    }
}
