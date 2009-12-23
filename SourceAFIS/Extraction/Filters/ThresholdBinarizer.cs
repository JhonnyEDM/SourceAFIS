using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SourceAFIS.General;

namespace SourceAFIS.Extraction.Filters
{
    public sealed class ThresholdBinarizer
    {
        public BinaryMap Binarize(float[,] input, float[,] baseline, BinaryMap mask, BlockMap blocks)
        {
            BinaryMap binarized = new BinaryMap(input.GetLength(1), input.GetLength(0));
            Threader.SplitY(blocks.BlockCount, delegate(Point block)
            {
                if (mask.GetBit(block))
                {
                    RectangleC rect = blocks.BlockAreas[block];
                    for (int y = rect.Bottom; y < rect.Top; ++y)
                        for (int x = rect.Left; x < rect.Right; ++x)
                            if (input[y, x] - baseline[y, x] > 0)
                                binarized.SetBitOne(x, y);
                }
            });
            Logger.Log(this, binarized);
            return binarized;
        }
    }
}
