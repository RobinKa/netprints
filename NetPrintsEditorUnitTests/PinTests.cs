using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetPrints.Graph;
using NetPrintsEditor.ViewModels;
using NetPrints.Core;
using System.Windows;

namespace NetPrintsEditorUnitTests
{
    [TestClass]
    public class PinTests
    {
        [TestMethod]
        public void TestRelativePositioning()
        {
            Method method = new Method("TestMethod");

            NodePinVM inExecPin = new NodePinVM(method.EntryNode.InitialExecutionPin);
            NodePinVM outExecPin = new NodePinVM(method.ReturnNode.ReturnPin);

            inExecPin.ConnectedPin = outExecPin;

            Assert.AreEqual(inExecPin.Pin, method.EntryNode.InitialExecutionPin);
            Assert.AreEqual(outExecPin.Pin, method.ReturnNode.ReturnPin);

            Assert.AreEqual(inExecPin.ConnectedPin, outExecPin);

            method.EntryNode.PositionX = 0;
            method.EntryNode.PositionY = 50;

            method.ReturnNode.PositionX = 150;
            method.ReturnNode.PositionY = 100;

            inExecPin.PositionX = 10;
            inExecPin.PositionY = 20;

            outExecPin.PositionX = 30;
            outExecPin.PositionY = 40;

            inExecPin.NodeRelativePosition = new Point(50, 60);
            outExecPin.NodeRelativePosition = new Point(70, 80);

            Assert.AreEqual(inExecPin.ConnectedPositionX, -50-0+150+70+30);

            Assert.AreEqual(inExecPin.ConnectedPositionY, -60-50+100+80+40);

            Assert.AreEqual(inExecPin.ConnectedPosition.X, inExecPin.ConnectedPositionX);
            Assert.AreEqual(inExecPin.ConnectedPosition.Y, inExecPin.ConnectedPositionY);
        }
    }
}
