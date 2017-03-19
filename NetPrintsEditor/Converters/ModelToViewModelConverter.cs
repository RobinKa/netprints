using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using NetPrintsEditor.ViewModels;
using NetPrints.Graph;
using System.Collections.ObjectModel;

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
            else if(value is ObservableCollection<Method> methods)
            {
                return new ObservableCollection<MethodVM>(methods.Select(m => new MethodVM(m)));
            }
            else if (value is ObservableCollection<Node> nodes)
            {
                return new ObservableCollection<NodeVM>(nodes.Select(n => new NodeVM(n)));
            }
            else if (value is ObservableCollection<Class> classes)
            {
                return new ObservableCollection<ClassVM>(classes.Select(c => new ClassVM(c)));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
