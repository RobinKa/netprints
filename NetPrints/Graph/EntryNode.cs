using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class EntryNode : Node
    {
        public NodeOutputExecPin InitialExecutionPin
        {
            get { return OutputExecPins[0]; }
        }

        public void SetArgumentTypes(IEnumerable<Type> parameterTypes)
        {
            foreach(NodeOutputDataPin pin in OutputDataPins)
            {
                GraphUtil.DisconnectOutputDataPin(pin);
            }

            OutputDataPins.Clear();

            foreach (Type paramType in parameterTypes)
            {
                AddOutputDataPin(paramType.Name, paramType);
            }
        }

        public EntryNode(Method method)
            : base(method)
        {
            AddOutputExecPin("Exec");

            SetArgumentTypes(Method.ArgumentTypes);
            Method.ArgumentTypes.CollectionChanged += OnArgumentTypesChanged;
        }

        private void OnArgumentTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetArgumentTypes(Method.ArgumentTypes);
        }

        public override string ToString()
        {
            return $"{Method.Name} Entry";
        }
    }
}
