using System;
using System.Collections.Generic;
using System.Text;

namespace NetPrints.Core
{
    public class Class
    {
        public IList<Variable> Attributes { get; } = new List<Variable>();
        public IList<Method> Methods { get; } = new List<Method>();

        public Type SuperType { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
    }
}
