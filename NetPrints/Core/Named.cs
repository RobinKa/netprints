using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetPrints.Core
{
    /// <summary>
    /// Contains a value and its name. Can be implicitly
    /// converted to the class itself.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public class Named<T>
    {
        public string Name { get; set; }
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
