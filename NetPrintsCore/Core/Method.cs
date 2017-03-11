using System;
using System.Collections.Generic;
using System.Text;
using NetPrints.Graph;

namespace NetPrints.Core
{
    public class Method
    {
        public MethodEntryNode MethodEntry
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public Type[] ArgumentTypes
        {
            get;
            private set;
        }

        public Type[] ReturnTypes
        {
            get;
            private set;
        }

        public Method(string name, Type[] argumentTypes, Type[] returnTypes, MethodEntryNode methodEntry)
        {
            Name = name;
            MethodEntry = methodEntry;
            ArgumentTypes = argumentTypes;
            ReturnTypes = returnTypes;
        }
    }
}
