using System;
using System.Collections.Generic;
using System.Text;

namespace NetPrints.Graph
{
    public static class GraphUtil
    {
        public static bool CanConnectNodePins(NodePin pinA, NodePin pinB, bool swapped=false)
        {
            return (pinA is NodeInputExecPin && pinB is NodeOutputExecPin) ||
                (pinA is NodeInputDataPin && pinB is NodeOutputDataPin) ||
                (!swapped && CanConnectNodePins(pinB, pinA, true));
        }

        public static void ConnectNodePins(NodePin pinA, NodePin pinB, bool swapped=false)
        {
            if (pinA is NodeInputExecPin exA && pinB is NodeOutputExecPin exB)
            {
                ConnectExecPins(exB, exA);
            }
            else if (pinA is NodeInputDataPin datA && pinB is NodeOutputDataPin datB)
            {
                ConnectDataPins(datB, datA);
            }
            else if (!swapped)
            {
                ConnectNodePins(pinB, pinA, true);
            }
            else
            {
                throw new ArgumentException("The passed pins can not be connected because their types are incompatible.");
            }
        }

        public static void ConnectExecPins(NodeOutputExecPin fromPin, NodeInputExecPin toPin)
        {
            // Remove from old pin if any
            if(fromPin.OutgoingPin != null)
            {
                fromPin.OutgoingPin.IncomingPins.Remove(fromPin);
            }

            fromPin.OutgoingPin = toPin;
            toPin.IncomingPins.Add(fromPin);
        }

        public static void ConnectDataPins(NodeOutputDataPin fromPin, NodeInputDataPin toPin)
        {
            // Remove from old pin if any
            if(toPin.IncomingPin != null)
            {
                toPin.IncomingPin.OutgoingPins.Remove(toPin);
            }

            fromPin.OutgoingPins.Add(toPin);
            toPin.IncomingPin = fromPin;
        }

        public static void DisconnectInputDataPin(NodeInputDataPin pin)
        {
            pin.IncomingPin?.OutgoingPins.Remove(pin);
            pin.IncomingPin = null;
        }

        public static void DisconnectOutputDataPin(NodeOutputDataPin pin)
        {
            foreach(NodeInputDataPin outgoingPin in pin.OutgoingPins)
            {
                outgoingPin.IncomingPin = null;
            }

            pin.OutgoingPins.Clear();
        }

        public static void DisconnectOutputExecPin(NodeOutputExecPin pin)
        {
            pin.OutgoingPin?.IncomingPins.Remove(pin);
            pin.OutgoingPin = null;
        }

        public static void DisconnectInputExecPin(NodeInputExecPin pin)
        {
            foreach (NodeOutputExecPin incomingPin in pin.IncomingPins)
            {
                incomingPin.OutgoingPin = null;
            }

            pin.IncomingPins.Clear();
        }
    }
}
