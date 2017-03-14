using System;
using System.Collections.ObjectModel;
using System.Text;

namespace NetPrints.Core
{
    public class Class
    {
        public ObservableCollection<Variable> Attributes { get; private set; } = new ObservableCollection<Variable>();
        public ObservableCollection<Method> Methods { get; private set; } = new ObservableCollection<Method>();

        public Type SuperType { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }

        public Class()
        {

        }
    }
}
