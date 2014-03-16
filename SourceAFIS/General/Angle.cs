using System;
using System.Collections.Generic;
using System.Text;

namespace SourceAFIS.General
{
    public static class Angle
    {
        public const double PI = (double)Math.PI;
        public const double PI2 = (double)(2 * Math.PI);
        public const byte PIB = 128;

        public const byte B180 = PIB;
        public const byte B90 = B180 / 2;
        public const byte B60 = B180 / 3;
        public const byte B45 = B180 / 4;
        public const byte B30 = B180 / 6;
        public const byte B15 = B180 / 12;

        public static double FromFraction(double fraction)
        {
            return fraction * PI2;
        }

        public static double ToFraction(double radians)
        {
            return radians / PI2;
        }

        public static byte ToByte(double angle)
        {
            return (byte)Quantize(angle, 256);
        }

        public static double ToFloat(byte angle)
        {
            return ByBucketCenter(angle, 256);
        }

        public static PointF ToVector(double angle)
        {
            return new PointF((double)Math.Cos(angle), (double)Math.Sin(angle));
        }

        public static PointF ToVector(byte angle)
        {
            return new PointF(Cos(angle), Sin(angle));
        }

        public static double ToOrientation(double direction)
        {
            if (direction < PI)
                return 2 * direction;
            else
                return 2 * (direction - PI);
        }

        public static byte ToOrientation(byte direction)
        {
            return (byte)(2 * direction);
        }

        public static byte ToDirection(byte orientation)
        {
            return (byte)(orientation / 2);
        }

        public static byte FromDegreesB(int degrees)
        {
            return (byte)((degrees * 256 + 180) / 360);
        }

        public static int ToDegrees(byte angle)
        {
            return (angle * 360 + 128) / 256;
        }

        public static double Atan(double x, double y)
        {
            double result = Math.Atan2(y, x);
            if (result < 0)
                result += 2 * Math.PI;
            return (double)result;
        }

        public static double Atan(PointF point)
        {
            return Atan(point.X, point.Y);
        }

        public static double Atan(Point point)
        {
            return Atan(point.X, point.Y);
        }

        public static byte AtanB(Point point)
        {
            return ToByte(Atan(point));
        }

        public static double Atan(Point center, Point point)
        {
            return Atan(Calc.Difference(point, center));
        }

        public static byte AtanB(Point center, Point point)
        {
            return ToByte(Atan(center, point));
        }

        static double[] PrecomputedSin = PrecomputeSin();

        static double[] PrecomputeSin()
        {
            double[] result = new double[256];
            for (int i = 0; i < 256; ++i)
                result[i] = (double)Math.Sin(ToFloat((byte)i));
            return result;
        }

        public static double Sin(byte angle)
        {
            return PrecomputedSin[angle];
        }

        static double[] PrecomputedCos = PrecomputeCos();

        static double[] PrecomputeCos()
        {
            double[] result = new double[256];
            for (int i = 0; i < 256; ++i)
                result[i] = (double)Math.Cos(ToFloat((byte)i));
            return result;
        }

        public static double Cos(byte angle)
        {
            return PrecomputedCos[angle];
        }

        public static double ByBucketBottom(int bucket, int resolution)
        {
            return FromFraction((double)bucket / (double)resolution);
        }

        public static double ByBucketTop(int bucket, int resolution)
        {
            return FromFraction((double)(bucket + 1) / (double)resolution);
        }

        public static double ByBucketCenter(int bucket, int resolution)
        {
            return FromFraction((double)(2 * bucket + 1) / (double)(2 * resolution));
        }

        public static int Quantize(double angle, int resolution)
        {
            int result = (int)(ToFraction(angle) * resolution);
            if (result < 0)
                return 0;
            else if (result >= resolution)
                return resolution - 1;
            else
                return result;
        }

        public static int Quantize(byte angle, int resolution)
        {
            return (int)angle * resolution / 256;
        }

        public static double Add(double angle1, double angle2)
        {
            double result = angle1 + angle2;
            if (result < PI2)
                return result;
            else
                return result - PI2;
        }

        public static byte Add(byte angle1, byte angle2)
        {
            return (byte)(angle1 + angle2);
        }

        public static byte Difference(byte angle1, byte angle2)
        {
            return (byte)(angle1 - angle2);
        }

        public static byte Distance(byte first, byte second)
        {
            byte diff = Difference(first, second);
            if (diff <= PIB)
                return diff;
            else
                return Complementary(diff);
        }

        public static byte Complementary(byte angle)
        {
            return (byte)-angle;
        }

        public static byte Opposite(byte angle)
        {
            return (byte)(angle + PIB);
        }

        const int PolarCacheBits = 8;
        const uint PolarCacheRadius = 1u << PolarCacheBits;
        const uint PolarCacheMask = PolarCacheRadius - 1;

        struct PolarPointB
        {
            public short Distance;
            public byte Angle;
        }

        static PolarPointB[,] PolarCache;

        static PolarPointB[,] CreatePolarCache()
        {
            PolarPointB[,] cache = new PolarPointB[PolarCacheRadius, PolarCacheRadius];
            for (int y = 0; y < PolarCacheRadius; ++y)
                for (int x = 0; x < PolarCacheRadius; ++x)
                {
                    cache[y, x].Distance = Convert.ToInt16(Math.Round(Math.Sqrt(Calc.Sq(x) + Calc.Sq(y))));
                    if (y > 0 || x > 0)
                        cache[y, x].Angle = Angle.AtanB(new Point(x, y));
                    else
                        cache[y, x].Angle = 0;
                }
            return cache;
        }

        public static PolarPoint ToPolar(Point point)
        {
            if (PolarCache == null)
                PolarCache = CreatePolarCache();
            
            int quadrant = 0;
            int x = point.X;
            int y = point.Y;

            if (y < 0)
            {
                x = -x;
                y = -y;
                quadrant = 128;
            }

            if (x < 0)
            {
                int tmp = -x;
                x = y;
                y = tmp;
                quadrant += 64;
            }

            int shift = Calc.HighestBit((uint)(x | y) >> PolarCacheBits);

            PolarPointB polarB = PolarCache[y >> shift, x >> shift];
            return new PolarPoint(polarB.Distance << shift, (byte)(polarB.Angle + quadrant));
        }
    }
}
