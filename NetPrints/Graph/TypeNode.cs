using NetPrints.Core;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class ObservableValue<T> : INotifyPropertyChanged
    {
        public delegate void ObservableValueChangedEventHandler(object sender, EventArgs eventArgs);

        [DataMember]
        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChanged?.Invoke(this, EventArgs.Empty);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        private T value;

        public ObservableValue(T value)
        {
            this.value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event ObservableValueChangedEventHandler OnValueChanged;

        public static implicit operator T (ObservableValue<T> observableValue)
        {
            return observableValue.Value;
        }

        public static implicit operator ObservableValue<T>(T value)
        {
            return new ObservableValue<T>(value);
        }
    }

    [DataContract]
    public class TypeNode : Node
    {
        [DataMember]
        public BaseType Type
        {
            get;
            private set;
        }

        [DataMember]
        private ObservableValue<BaseType> constructedType;

        public TypeNode(Method method, BaseType type)
            : base(method)
        {
            Type = type;

            // Add type pins for each generic argument of the literal type
            // and monitor them for changes to reconstruct the actual pin types.
            if (Type is TypeSpecifier typeSpecifier)
            {
                foreach (var genericArg in typeSpecifier.GenericArguments.OfType<GenericType>())
                {
                    AddInputTypePin(genericArg.Name);
                }
            }

            constructedType = new ObservableValue<BaseType>(GetConstructedOutputType());
            AddOutputTypePin("OutputType", constructedType);
        }

        protected override void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            base.OnInputTypeChanged(sender, eventArgs);

            // Set the type of the output type pin by constructing
            // the type of this node with the input type pins.
            constructedType.Value = GetConstructedOutputType();
        }

        private BaseType GetConstructedOutputType()
        {
            return GenericsHelper.ConstructWithTypePins(Type, InputTypePins);
        }

        public override string ToString()
        {
            return Type.ShortName;
        }
    }
}
