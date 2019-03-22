using NetPrints.Core;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace NetPrintsEditor.Converters
{
    [ValueConversion(typeof(MethodInfo), typeof(string))]
    public class MethodSpecifierConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is MethodSpecifier methodSpecifier)
            {
                string name;

                // Check if the method is an operator
                if (OperatorUtil.TryGetOperatorInfo(methodSpecifier, out OperatorInfo operatorInfo))
                {
                    name = $"Operator {operatorInfo.DisplayName}";
                }
                else
                {
                    name = methodSpecifier.Name;
                }

                string paramTypeNames = string.Join(", ", methodSpecifier.Arguments);
                string s = $"{methodSpecifier.DeclaringType} {name} ({paramTypeNames})";

                if(methodSpecifier.ReturnTypes.Count > 0)
                {
                    s += $" : {string.Join(", ", methodSpecifier.ReturnTypes)}";
                }

                return s;
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
