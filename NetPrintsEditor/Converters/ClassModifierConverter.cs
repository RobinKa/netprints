using NetPrints.Core;
using System;
using System.Globalization;
using System.Windows.Data;

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

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is ClassModifiers mask)
            {
                modifiers ^= mask;
                return modifiers;
            }

            throw new NotImplementedException();
        }
    }
}
