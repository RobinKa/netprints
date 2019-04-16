using System.Runtime.Serialization;

namespace NetPrints.Core
{
    /// <summary>
    /// Contains a value and its name. Can be implicitly
    /// converted to the class itself.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    [DataContract]
    [KnownType(typeof(MethodParameter))]
    public class Named<T>
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public T Value { get; set; }

        public Named(string name, T type)
        {
            Name = name;
            Value = type;
        }

        public static implicit operator T (Named<T> namedValue) => namedValue.Value;

        public override string ToString()
        {
            return $"{Name}: {Value}";
        }
    }
}
