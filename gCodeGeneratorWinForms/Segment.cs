using System;
using System.Collections.Generic;
using System.Text;

namespace gCodeGeneratorWinForms
{
    public class Segment
    {
        public MoveType Type;
        public PointF Start, End;
        public bool IsArc;
        public PointF Center;
        public float Radius;
        public float StartAngle, SweepAngle;
    }

    public enum MoveType { Rapid, Feed, Arc }
}
