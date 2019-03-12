using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Controls;
using NetPrintsEditor.Dialogs;
using NetPrintsEditor.Reflection;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace NetPrintsEditor.Converters
{
    public class SuggestionListConverter : IValueConverter
    {
        private MethodSpecifierConverter methodSpecifierConverter = new MethodSpecifierConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = "";
            string iconPath = "";

            if (value is MethodSpecifier methodSpecifier)
            {
                text = methodSpecifierConverter.Convert(methodSpecifier, typeof(string), parameter, culture) as string;
                iconPath = "Method_16x.png";
            }
            else if (value is PropertySpecifier propertySpecifier)
            {
                text = $"{propertySpecifier.DeclaringType} {propertySpecifier.Name} : {propertySpecifier.Type}";
                iconPath = "Property_16x.png";
            }
            else if (value is MakeDelegateTypeInfo makeDelegateTypeInfo)
            {
                text = $"NetPrints - Make Delegate For A Method Of {makeDelegateTypeInfo.Type.ShortName}";
                iconPath = "Delegate_16x.png";
            }
            else if (value is TypeSpecifier t)
            {
                if (t == TypeSpecifier.FromType<ForLoopNode>())
                {
                    text = "NetPrints - For Loop";
                    iconPath = "Loop_16x.png";
                }
                else if (t == TypeSpecifier.FromType<IfElseNode>())
                {
                    text = "NetPrints - If Else";
                    iconPath = "If_16x.png";
                }
                else if (t == TypeSpecifier.FromType<ConstructorNode>())
                {
                    text = "NetPrints - Construct New Object";
                    iconPath = "Create_16x.png";
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            if (targetType == typeof(string))
            {
                return text;
            }
            else
            {
                var listItem = new SuggestionListItem();
                listItem.Text = text;

                // See https://docs.microsoft.com/en-us/dotnet/framework/wpf/app-development/pack-uris-in-wpf for format
                listItem.IconPath = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/Resources/{iconPath}";

                return listItem;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
