using NetPrints.Graph;
using PropertyChanged;
using System.Drawing;

namespace NetPrints.Base
{
    [AddINotifyPropertyChangedInterface]
    public class PinConnection
    {
        public NodePin PinA { get; }
        public NodePin PinB { get; }

        public Point PointA { get; private set; }
        public Point PointB { get; private set; }
        public Point PointC { get; private set; }
        public Point PointD { get; private set; }

        private const double CPOffset = 100;

        public PinConnection(NodePin a, NodePin b)
        {
            PinA = a;
            PinB = b;

            PinA.PositionChanged += (sender, e) => RecalculatePoints();
            PinB.PositionChanged += (sender, e) => RecalculatePoints();

            RecalculatePoints();
        }

        private void RecalculatePoints()
        {
            PointA = new Point((int)PinA.PositionX, (int)PinA.PositionY);
            PointD = new Point((int)PinB.PositionX, (int)PinB.PositionY);

            int sign = PinA is INodeOutputPin ? 1 : -1;

            PointB = new Point((int)(PinA.PositionX + sign * CPOffset), (int)PinA.PositionY);
            PointC = new Point((int)(PinB.PositionX - sign * CPOffset), (int)PinB.PositionY);
        }
    }
}
