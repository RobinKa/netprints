using NetPrints.Core;
using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace NetPrints.Serialization
{
    public class TypeWrapper
    {
        public string TypeName;
    }

    public class TypeReplacementSurrogate : IDataContractSurrogate
    {
        public Type GetDataContractType(Type type)
        {
            if (type == typeof(Type))
            {
                return typeof(TypeWrapper);
            }

            return type;
        }

        public object GetDeserializedObject(object obj, Type targetType)
        {
            if (obj is TypeWrapper wrapper)
            {
                return NetPrintsUtil.GetTypeFromFullName(wrapper.TypeName);
            }

            return obj;
        }

        public object GetObjectToSerialize(object obj, Type targetType)
        {
            if (obj is Type t)
            {
                return new TypeWrapper() { TypeName = t.FullName };
            }

            return obj;
        }

        public object GetCustomDataToExport(Type clrType, Type dataContractType)
        {
            return null;
        }

        public object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType)
        {
            return null;
        }

        public void GetKnownCustomDataTypes(Collection<Type> customDataTypes)
        {

        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
        {
            return null;
        }

        public CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit)
        {
            return null;
        }
    }
}
