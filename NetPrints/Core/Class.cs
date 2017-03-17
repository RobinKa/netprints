using System;
using System.Collections.ObjectModel;
using System.Text;

namespace NetPrints.Core
{
    [Flags]
    public enum ClassModifiers
    {
        Private = 0,
        Public = 1,
        Protected = 2,
        Internal = 4,
        Sealed = 8,
        Abstract = 16,
        Static = 32,
    }

    public class Class
    {
        public ObservableCollection<Variable> Attributes { get; private set; } = new ObservableCollection<Variable>();
        public ObservableCollection<Method> Methods { get; private set; } = new ObservableCollection<Method>();

        public Type SuperType { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }

        public ClassModifiers Modifiers { get; set; } = ClassModifiers.Internal;

        public Class()
        {

        }
    }
}
