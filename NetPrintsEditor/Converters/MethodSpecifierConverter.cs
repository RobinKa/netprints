using NetPrints.Core;
using NetPrintsEditor.Reflection;
using System;
using System.Globalization;
using System.Linq;
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
                string paramTypeNames = string.Join(", ", methodSpecifier.Arguments);

                string s = $"{methodSpecifier.DeclaringType} {methodSpecifier.Name} ({paramTypeNames})";

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
