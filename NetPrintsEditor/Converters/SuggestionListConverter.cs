using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Dialogs;
using NetPrintsEditor.Reflection;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace NetPrintsEditor.Converters
{
    public class SuggestionListConverter : IValueConverter
    {
        private MethodSpecifierConverter methodSpecifierConverter = new MethodSpecifierConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MethodSpecifier methodSpecifier)
            {
                return methodSpecifierConverter.Convert(methodSpecifier, targetType, parameter, culture);
            }
            else if(value is PropertySpecifier propertySpecifier)
            {
                return $"{propertySpecifier.DeclaringType} {propertySpecifier.Name} : {propertySpecifier.Type}";
            }
            else if(value is MakeDelegateTypeInfo makeDelegateTypeInfo)
            {
                return $"NetPrints - Make Delegate For A Method Of {makeDelegateTypeInfo.Type.ShortName}";
            }
            else if (value is TypeSpecifier t)
            {
                if (t == TypeSpecifier.FromType<ForLoopNode>())
                {
                    return "NetPrints - For Loop";
                }
                else if (t == TypeSpecifier.FromType<IfElseNode>())
                {
                    return "NetPrints - If Else";
                }
                else if(t == TypeSpecifier.FromType<ConstructorNode>())
                {
                    return "NetPrints - Construct New Object";
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
