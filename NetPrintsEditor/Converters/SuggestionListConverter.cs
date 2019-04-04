using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Controls;
using NetPrintsEditor.Dialogs;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace NetPrintsEditor.Converters
{
    public class SuggestionListConverter : IValueConverter
    {
        private MethodSpecifierConverter methodSpecifierConverter = new MethodSpecifierConverter();

        public object Convert(object tupleObject, Type targetType, object parameter, CultureInfo culture)
        {
            string text = "";
            string iconPath = "";

            SearchableComboBoxItem item = (SearchableComboBoxItem)tupleObject;
            string category = item.Category;
            object value = item.Value;

            if (value is MethodSpecifier methodSpecifier)
            {
                text = methodSpecifierConverter.Convert(methodSpecifier, typeof(string), parameter, culture) as string;

                iconPath = OperatorUtil.IsOperator(methodSpecifier) ? "Operator_16x.png" : "Method_16x.png";
            }
            else if (value is VariableSpecifier variableSpecifier)
            {
                text = $"{variableSpecifier.Type} {variableSpecifier.Name} : {variableSpecifier.Type}";
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
                else if (t == TypeSpecifier.FromType<TypeOfNode>())
                {
                    text = "NetPrints - Type Of";
                    iconPath = "Type_16x.png";
                }
                else if (t == TypeSpecifier.FromType<ExplicitCastNode>())
                {
                    text = "NetPrints - Explicit Cast";
                    iconPath = "Convert_16x.png";
                }
                else if (t == TypeSpecifier.FromType<ReturnNode>())
                {
                    text = "NetPrints - Return";
                    iconPath = "Return_16x.png";
                }
                else if (t == TypeSpecifier.FromType<MakeArrayNode>())
                {
                    text = "NetPrints - Make Array";
                    iconPath = "ListView_16x.png";
                }
                else if (t == TypeSpecifier.FromType<LiteralNode>())
                {
                    text = "NetPrints - Literal";
                    iconPath = "Literal_16x.png";
                }
                else if (t == TypeSpecifier.FromType<TypeNode>())
                {
                    text = "NetPrints - Type";
                    iconPath = "Type_16x.png";
                }
                else if (t == TypeSpecifier.FromType<MakeArrayTypeNode>())
                {
                    text = "NetPrints - Make Array Type";
                    iconPath = "Type_16x.png";
                }
                else if (t == TypeSpecifier.FromType<ThrowNode>())
                {
                    text = "NetPrints - Throw";
                    iconPath = "Throw_16x.png";
                }
                else
                {
                    text = t.FullCodeName;
                    iconPath = "Type_16x.png";
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
                listItem.Category = category;
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
