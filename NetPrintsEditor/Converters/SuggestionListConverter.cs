using NetPrints.Graph;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace NetPrintsEditor.Converters
{
    public class SuggestionListConverter : IValueConverter
    {
        private MethodInfoConverter methodInfoConverter = new MethodInfoConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MethodInfo methodInfo)
            {
                return methodInfoConverter.Convert(methodInfo, targetType, parameter, culture);
            }
            else if (value is Type t)
            {
                if (t == typeof(ForLoopNode))
                {
                    return "Control Flow - For Loop";
                }
                else if (t == typeof(IfElseNode))
                {
                    return "Control Flow - If Else";
                }
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
