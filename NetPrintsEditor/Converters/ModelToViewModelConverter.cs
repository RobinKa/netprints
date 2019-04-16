using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Compilation;
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
            if (value is NodeGraph graph)
            {
                return new NodeGraphVM(graph);
            }
            else if (value is Node node)
            {
                return new NodeVM(node);
            }
            else if (value is Class cls)
            {
                return new ClassVM(cls);
            }
            else if (value is NodePin pin)
            {
                return new NodePinVM(pin);
            }
            else if (value is CompilationReference reference)
            {
                return new CompilationReferenceVM(reference);
            }

            throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
