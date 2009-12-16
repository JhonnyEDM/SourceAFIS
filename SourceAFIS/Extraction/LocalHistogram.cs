using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SourceAFIS.General;

namespace SourceAFIS.Extraction
{
    public sealed class LocalHistogram
    {
        public short[, ,] Analyze(BlockMap blocks, byte[,] image)
        {
            short[, ,] histogram = new short[blocks.BlockCount.Height, blocks.BlockCount.Width, 256];
            Threader.Split<Point>(blocks.BlockList, delegate(Point block)
            {
                RectangleC area = blocks.BlockAreas[block.Y, block.X];
                for (int y = area.Bottom; y < area.Top; ++y)
                    for (int x = area.Left; x < area.Right; ++x)
                        ++histogram[block.Y, block.X, image[y, x]];
            });
            return histogram;
        }

        public short[, ,] SmoothAroundCorners(BlockMap blocks, short[, ,] input)
        {
            Point[] blocksAround = new Point[] { new Point(0, 0), new Point(-1, 0), new Point(0, -1), new Point(-1, -1) };
            short[, ,] output = new short[blocks.CornerCount.Height, blocks.CornerCount.Width, 256];
            Threader.Split<Point>(blocks.CornerList, delegate(Point corner)
            {
                foreach (Point relative in blocksAround)
                {
                    Point block = Calc.Add(corner, relative);
                    if (blocks.BlockRect.Contains(block))
                    {
                        for (int i = 0; i < 256; ++i)
                            output[corner.Y, corner.X, i] += input[block.Y, block.X, i];
                    }
                }
            });
            return output;
        }

        public short[, ,] Smooth(BlockMap blocks, short[, ,] input)
        {
            short[, ,] output = new short[blocks.CornerCount.Height, blocks.CornerCount.Width, 256];
            Threader.Split<Point>(blocks.CornerList, delegate(Point corner)
            {
                for (int i = 0; i < 256; ++i)
                    output[corner.Y, corner.X, i] = input[corner.Y, corner.X, i];
                foreach (Point neigborRelative in Neighborhood.CornerNeighbors)
                {
                    Point neighbor = Calc.Add(corner, neigborRelative);
                    if (blocks.CornerRect.Contains(neighbor))
                    {
                        for (int i = 0; i < 256; ++i)
                            output[corner.Y, corner.X, i] += input[neighbor.Y, neighbor.X, i];
                    }
                }
            });
            return output;
        }
    }
}