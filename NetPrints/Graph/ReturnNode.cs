using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    [DataContract]
    public class ReturnNode : Node
    {
        public NodeInputExecPin ReturnPin
        {
            get { return InputExecPins[0]; }
        }

        public void SetReturnTypes(IEnumerable<Type> returnTypes)
        {
            foreach (NodeInputDataPin pin in InputDataPins)
            {
                GraphUtil.DisconnectInputDataPin(pin);
            }

            InputDataPins.Clear();

            foreach (Type returnType in returnTypes)
            {
                AddInputDataPin(returnType.Name, returnType);
            }
        }

        public ReturnNode(Method method)
            : base(method)
        {
            AddInputExecPin("Exec");

            SetReturnTypes(method.ReturnTypes);
            Method.ReturnTypes.CollectionChanged += OnReturnTypesChanged;
        }

        private void OnReturnTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetReturnTypes(Method.ReturnTypes);
        }
    }
}
