using NetPrints.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NetPrintsEditor.Converters
{
    [ValueConversion(typeof(VariableModifiers), typeof(string))]
    public class VariableModifierConverter : IValueConverter
    {
        VariableModifiers modifiers;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VariableModifiers m && parameter is VariableModifiers mask)
            {
                modifiers = m;
                return (mask & m) != 0;
            }

            return VariableModifiers.Private;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is VariableModifiers mask)
            {
                modifiers ^= mask;
                return modifiers;
            }

            return VariableModifiers.Private;
        }
    }
}
