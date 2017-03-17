using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
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

    [DataContract]
    public class Class
    {
        [DataMember]
        public ObservableCollection<Variable> Attributes { get; set; } = new ObservableCollection<Variable>();

        [DataMember]
        public ObservableCollection<Method> Methods { get; set; } = new ObservableCollection<Method>();

        [DataMember]
        public Type SuperType { get; set; } = typeof(object);

        [DataMember]
        public string Namespace { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ClassModifiers Modifiers { get; set; } = ClassModifiers.Internal;

        public Class()
        {

        }
    }
}
