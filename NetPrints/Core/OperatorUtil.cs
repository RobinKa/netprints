using System;
using System.Collections.Generic;
using System.Text;

namespace NetPrints.Core
{
    public class OperatorInfo
    {
        public string DisplayName { get; }
        public string Symbol { get; }
        public bool Unary { get; }
        public bool UnaryRightPosition { get; }

        public OperatorInfo(string displayName, string symbol, bool unary, bool unaryRightPosition = false)
        {
            DisplayName = displayName;
            Symbol = symbol;
            Unary = unary;
            UnaryRightPosition = unaryRightPosition;
        }
    }

    public static class OperatorUtil
    {
        private const string OperatorPrefix = "op_";

        /// <summary>
        /// Mapping from operator method name to operator definitions (display name, symbol, arity, position).
        /// </summary>
        private static readonly Dictionary<string, OperatorInfo> operatorSymbols = new Dictionary<string, OperatorInfo>()
        {
            // Unary
            ["op_Increment"] = new OperatorInfo("Increment", "++", true, true),
            ["op_Decrement"] = new OperatorInfo("Decrement", "--", true, true),
            ["op_UnaryPlus"] = new OperatorInfo("Unary Plus", "+", true),
            ["op_UnaryNegation"] = new OperatorInfo("Unary Negation", "-", true),
            ["op_LogicalNot"] = new OperatorInfo("Not", "!", true),

            // Binary
            ["op_Addition"] = new OperatorInfo("Add", "+", false),
            ["op_Subtraction"] = new OperatorInfo("Subtract", "-", false),
            ["op_Multiply"] = new OperatorInfo("Multiply", "*", false),
            ["op_Division"] = new OperatorInfo("Divide", "/", false),
            ["op_Modulus"] = new OperatorInfo("Modulus", "%", false),
            ["op_GreaterThan"] = new OperatorInfo("Greater than", ">", false),
            ["op_GreaterThanOrEqual"] = new OperatorInfo("Greater than or equal", ">=", false),
            ["op_Equality"] = new OperatorInfo("Equal", "==", false),
            ["op_Inequality"] = new OperatorInfo("Not Equal", "!=", false),
            ["op_LessThan"] = new OperatorInfo("Less than", "<", false),
            ["op_LessThanOrEqual"] = new OperatorInfo("Less than or equal", "<=", false),
            ["op_BitwiseAnd"] = new OperatorInfo("Bitwise AND", "&", false),
            ["op_BitwiseOr"] = new OperatorInfo("Bitwise OR", "|", false),
            ["op_ExclusiveOr"] = new OperatorInfo("Bitwise XOR", "^", false),
            ["op_LeftShift"] = new OperatorInfo("Shift Left", "<<", false),
            ["op_RightShift"] = new OperatorInfo("Shift Right", ">>", false),

            // Custom (not part of .NET symbols)
            ["op_BitwiseNot"] = new OperatorInfo("Bitwise NOT", "~", true),
            ["op_LogicalAnd"] = new OperatorInfo("And", "&&", false),
            ["op_LogicalOr"] = new OperatorInfo("Or", "||", false),
        };

        /// <summary>
        /// Returns whether the method specifier is an operator.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        public static bool IsOperator(MethodSpecifier methodSpecifier) =>
            operatorSymbols.ContainsKey(methodSpecifier.Name);

        /// <summary>
        /// Tries to get operator info for a method specifier.
        /// </summary>
        /// <param name="methodSpecifier">Method specifier to find operator info for.</param>
        /// <param name="operatorInfo">Operator info for the method specifier if found.</param>
        /// <returns></returns>
        public static bool TryGetOperatorInfo(MethodSpecifier methodSpecifier, out OperatorInfo operatorInfo) =>
            operatorSymbols.TryGetValue(methodSpecifier.Name, out operatorInfo);
    }
}
