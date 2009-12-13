using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace SourceAFIS.General
{
    public class Neighborhood
    {
        public static readonly Point[] EdgeNeighbors = new Point[] {
            new Point(0, -1),
            new Point(-1, 0),
            new Point(1, 0),
            new Point(0, 1)
        };

        public static readonly Point[] CornerNeighbors = new Point[] {
            new Point(-1, -1),
            new Point(0, -1),
            new Point(1, -1),
            new Point(-1, 0),
            new Point(1, 0),
            new Point(-1, 1),
            new Point(0, 1),
            new Point(1, 1)
        };
    }
}
