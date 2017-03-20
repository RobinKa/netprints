using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NetPrintsEditor.Converters
{
    public class ModelToViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is Method method)
            {
                return new MethodVM(method);
            }
            else if(value is Node node)
            {
                return new NodeVM(node);
            }
            else if(value is Class cls)
            {
                return new ClassVM(cls);
            }
            else if(value is NodePin pin)
            {
                return new NodePinVM(pin);
            }

            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
