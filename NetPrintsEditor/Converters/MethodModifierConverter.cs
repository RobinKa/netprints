using NetPrints.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NetPrintsEditor.Converters
{
    [ValueConversion(typeof(MethodModifiers), typeof(string))]
    public class MethodModifierConverter : IValueConverter
    {
        private MethodModifiers modifiers;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MethodModifiers m && parameter is MethodModifiers mask)
            {
                modifiers = m;
                return (mask & m) != 0;
            }

            return MethodModifiers.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is MethodModifiers mask)
            {
                modifiers ^= mask;
                return modifiers;
            }

            return MethodModifiers.None;
        }
    }
}
