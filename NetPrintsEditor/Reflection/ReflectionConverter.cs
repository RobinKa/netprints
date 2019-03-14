using Microsoft.CodeAnalysis;
using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetPrintsEditor.Reflection
{
    public static class ReflectionConverter
    {
        public static TypeSpecifier TypeSpecifierFromSymbol(ITypeSymbol type)
        {
            string typeName;

            if (type is IArrayTypeSymbol arrayType)
            {
                typeName = typeof(Array).FullName;
            }
            else
            {
                // Get the nested name (represented by + between classes)
                // See https://stackoverflow.com/questions/2443244/having-a-in-the-class-name
                string nestedPrefix = "";
                ITypeSymbol containingType = type.ContainingType;
                while (containingType != null)
                {
                    nestedPrefix = $"{containingType.Name}+{nestedPrefix}";
                    containingType = containingType.ContainingType;
                }

                typeName = nestedPrefix + type.Name.Split('`').First();
                if (type.ContainingNamespace != null)
                {
                    typeName = type.ContainingNamespace + "." + typeName;
                }
            }

            TypeSpecifier typeSpecifier = new TypeSpecifier(typeName,
                    type.TypeKind == TypeKind.Enum,
                    type.TypeKind == TypeKind.Interface);

            if (type is INamedTypeSymbol namedType)
            {
                if (namedType.IsUnboundGenericType)
                {
                    throw new ArgumentException(nameof(type));
                }

                foreach (ITypeSymbol genType in namedType.TypeArguments)
                {
                    if (genType is ITypeParameterSymbol genTypeParam)
                    {
                        // TODO: Convert and add constraints
                        typeSpecifier.GenericArguments.Add(GenericTypeSpecifierFromSymbol(genTypeParam));
                    }
                    else
                    {
                        typeSpecifier.GenericArguments.Add(TypeSpecifierFromSymbol(genType));
                    }
                }
            }
            
            

            return typeSpecifier;
        }

        public static GenericType GenericTypeSpecifierFromSymbol(ITypeParameterSymbol type)
        {
            // TODO: Convert constraints
            GenericType genericType = new GenericType(type.Name);

            return genericType;
        }

        public static BaseType BaseTypeSpecifierFromSymbol(ITypeSymbol type)
        {
            if (type is ITypeParameterSymbol typeParam)
            {
                return GenericTypeSpecifierFromSymbol(typeParam);
            }
            else
            {
                return TypeSpecifierFromSymbol(type);
            }
        }

        public static MethodSpecifier MethodSpecifierFromSymbol(IMethodSymbol method)
        {
            MethodModifiers modifiers = MethodModifiers.Private;

            if (method.IsPublic())
            {
                modifiers |= MethodModifiers.Public;
            }

            if (method.IsVirtual)
            {
                modifiers |= MethodModifiers.Virtual;
            }

            if (method.IsSealed)
            {
                modifiers |= MethodModifiers.Sealed;
            }

            if (method.IsAbstract)
            {
                modifiers |= MethodModifiers.Abstract;
            }

            if (method.IsStatic)
            {
                modifiers |= MethodModifiers.Static;
            }

            // TODO: Protected / Internal

            BaseType[] returnTypes = method.ReturnsVoid ?
                new BaseType[] { } :
                new BaseType[] { BaseTypeSpecifierFromSymbol(method.ReturnType) };
            

            BaseType[] parameterTypes = method.Parameters.Select(
                p => BaseTypeSpecifierFromSymbol(p.Type)).ToArray();

            BaseType[] genericArgs = method.TypeParameters.Select(
                p => BaseTypeSpecifierFromSymbol(p)).ToArray();

            return new MethodSpecifier(
                method.Name,
                parameterTypes,
                returnTypes,
                modifiers,
                TypeSpecifierFromSymbol(method.ContainingType),
                genericArgs);
        }

        public static PropertySpecifier PropertySpecifierFromSymbol(IPropertySymbol property)
        {
            bool hasPublicGetter = property.GetMethod != null ? property.GetMethod.IsPublic() : false;
            bool hasPublicSetter = property.SetMethod != null ? property.SetMethod.IsPublic() : false;

            return new PropertySpecifier(
                property.Name,
                TypeSpecifierFromSymbol(property.Type),
                hasPublicGetter,
                hasPublicSetter,
                TypeSpecifierFromSymbol(property.ContainingType));
        }

        public static PropertySpecifier PropertySpecifierFromField(IFieldSymbol field)
        {
            // TODO: Create own specifier for fields / unify with properties

            return new PropertySpecifier(
                field.Name,
                TypeSpecifierFromSymbol(field.Type),
                field.IsPublic(),
                field.IsPublic() ? !field.IsReadOnly : false,
                TypeSpecifierFromSymbol(field.ContainingType)
            );
        }

        public static ConstructorSpecifier ConstructorSpecifierFromSymbol(IMethodSymbol constructorMethodSymbol)
        {
            return new ConstructorSpecifier(
                constructorMethodSymbol.Parameters.Select(p => TypeSpecifierFromSymbol(p.Type)),
                TypeSpecifierFromSymbol(constructorMethodSymbol.ContainingType));
        }
    }
}
