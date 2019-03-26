using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetPrints.Core;

namespace NetPrintsEditor.Reflection
{
    public static class DefaultOperatorSpecifiers
    {
        public static IEnumerable<MethodSpecifier> All
        {
            get
            {
                if (all == null)
                {
                    all = new List<MethodSpecifier>();

                    // Numerical
                    foreach (var defaultNumericType in defaultNumericTypes)
                    {
                        foreach (var unaryOpName in defaultNumericUnaryOperatorNames)
                        {
                            AddOperator(unaryOpName, true, defaultNumericType, defaultNumericType);
                        }

                        foreach (var unaryOpName in defaultNumericBinaryOperatorNames)
                        {
                            AddOperator(unaryOpName, false, defaultNumericType, defaultNumericType);
                        }
                    }

                    // Logical (boolean)
                    var boolType = TypeSpecifier.FromType<bool>();
                    AddOperator("op_LogicalNot", true, boolType, boolType);
                    AddOperator("op_LogicalAnd", false, boolType, boolType);
                    AddOperator("op_LogicalOr", false, boolType, boolType);

                    // Integer bitwise operators
                    var intType = TypeSpecifier.FromType<int>();
                    AddOperator("op_LogicalNot", true, intType, intType);
                    AddOperator("op_BitwiseAnd", false, intType, intType);
                    AddOperator("op_BitwiseOr", false, intType, intType);
                    AddOperator("op_ExclusiveOr", false, intType, intType);
                    AddOperator("op_LeftShift", false, intType, intType);
                    AddOperator("op_RightShift", false, intType, intType);

                    // String addition
                    var stringType = TypeSpecifier.FromType<string>();
                    AddOperator("op_Addition", false, stringType, stringType);
                }

                return all;
            }
        }

        private static List<MethodSpecifier> all;

        private static void AddOperator(string opName, bool unary, TypeSpecifier argType, TypeSpecifier returnType)
        {
            IEnumerable<MethodParameter> parameters = new[]
            {
                    new MethodParameter("a", argType, MethodParameterPassType.Default),
            };

            if (!unary)
            {
                parameters = parameters.Concat(new[] { new MethodParameter("b", argType, MethodParameterPassType.Default) });
            }

            all.Add(new MethodSpecifier(opName, parameters, new[] { returnType }, MethodModifiers.Static, MemberVisibility.Public, returnType, new BaseType[0]));
        }

        private static readonly List<TypeSpecifier> defaultNumericTypes = new List<TypeSpecifier>()
        {
            TypeSpecifier.FromType<byte>(),
            TypeSpecifier.FromType<short>(),
            TypeSpecifier.FromType<ushort>(),
            TypeSpecifier.FromType<int>(),
            TypeSpecifier.FromType<uint>(),
            TypeSpecifier.FromType<float>(),
            TypeSpecifier.FromType<double>(),
            TypeSpecifier.FromType<decimal>(),
        };

        private static readonly IEnumerable<string> defaultNumericBinaryOperatorNames = new[]
        {
            "op_Addition",
            "op_Subtraction",
            "op_Multiply",
            "op_Division",
            "op_Modulus",
        };

        private static readonly IEnumerable<string> defaultNumericUnaryOperatorNames = new[]
        {
            "op_Increment",
            "op_Decrement",
            "op_UnaryPlus",
            "op_UnaryNegation",
        };
    }
}
