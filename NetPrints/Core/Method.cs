using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NetPrints.Graph;

namespace NetPrints.Core
{
    [Flags]
    public enum MethodModifiers
    {
        Private = 0,
        Public = 1,
        Protected = 2,
        Internal = 4,
        Sealed = 8,
        Abstract = 16,
        Static = 32,
        Virtual = 64,
        Override = 128,
    }

    public class Method
    {
        public EntryNode EntryNode
        {
            get;
            private set;
        }

        public ReturnNode ReturnNode
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            set;
        }

        public ObservableCollection<Node> Nodes
        {
            get;
            private set;
        } = new ObservableCollection<Node>();

        public ObservableCollection<Type> ArgumentTypes
        {
            get;
            private set;
        } = new ObservableCollection<Type>();

        public ObservableCollection<Type> ReturnTypes
        {
            get;
            private set;
        } = new ObservableCollection<Type>();

        public MethodModifiers Modifiers
        {
            get;
            set;
        } = MethodModifiers.Private;

        public Method(string name)
        {
            Name = name;
            EntryNode = new EntryNode(this);
            ReturnNode = new ReturnNode(this);
        }
    }
}
