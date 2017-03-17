using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;

namespace NetPrintsEditor.Converters
{
    [ValueConversion(typeof(ClassModifiers), typeof(string))]
    public class ClassModifierConverter : IValueConverter
    {
        ClassModifiers modifiers;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ClassModifiers m && parameter is ClassModifiers mask)
            {
                modifiers = m;
                return (mask & m) != 0;
            }

            return ClassModifiers.Internal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is ClassModifiers mask)
            {
                modifiers ^= mask;
                return modifiers;
            }

            return ClassModifiers.Internal;
        }
    }
}
