﻿#region BSD license
/*
Copyright © 2015, KimikoMuffin.
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.
3. The names of its contributors may not be used to endorse or promote 
   products derived from this software without specific prior written 
   permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace URandomGen.Tests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ConsoleKey key;

            do
            {
                const int _imgSize = 640;
                const int _maxVals = _imgSize * _imgSize / 32;

                Console.WriteLine("Each test involves the generation of " + _maxVals + " random numbers; however, not all values will be used for all tests.");
                Console.WriteLine("1. RandomCMWC");
                Console.WriteLine("2. RandomMersenne");
                Console.WriteLine("3. RandomXorShift");
                Console.WriteLine("4. System.Random, for comparison");
                Console.WriteLine("5. Test RandomGen methods");
                Console.WriteLine("X. Exit");

                key = ReadKey();

                Random generator = null;

                switch (key)
                {
                    case ConsoleKey.D1:
                        generator = new RandomCMWC();
                        break;
                    case ConsoleKey.D2:
                        generator = new RandomMersenne();
                        break;
                    case ConsoleKey.D3:
                        generator = new RandomXorshift();
                        break;
                    case ConsoleKey.D4:
                        generator = new Random();
                        break;
                    case ConsoleKey.D5:
                        {
                            RandomSequence rSeq = new RandomSequence();
                            Console.WriteLine("  Testing random whether random sequences properly fall in range:");
                            bool good = true;

                            good &= _testResult(4, int.MaxValue - 4, rSeq.Next, "Next");
                            good &= _testResult(1 << 24, rSeq.Next, "Next");
                            good &= _testResult<int>(ushort.MaxValue, 1 << 24, rSeq.Next, "Next");
                            good &= _testResult<int>(ushort.MaxValue, rSeq.Next, "Next");
                            good &= _testResult<int>(413, 414, rSeq.Next, "Next");
                            good &= _testResult<int>(413, 413, rSeq.Next, "Next");

                            good &= _testResult<uint>(4u, uint.MaxValue - 4, rSeq.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(1u << 24, rSeq.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(ushort.MaxValue, 1u << 24, rSeq.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(ushort.MaxValue, rSeq.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(413, 414, rSeq.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(413, 413, rSeq.NextUInt32, "NextUInt32");

                            good &= _testResult<uint>(4u, uint.MaxValue - 4, rSeq, RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(4u, uint.MaxValue - 4, new Random(1), RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(1u << 24, rSeq, RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(1u << 24, new Random(1), RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(ushort.MaxValue, 1u << 24, rSeq, RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(ushort.MaxValue, 1u << 24, new Random(1), RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(ushort.MaxValue, rSeq, RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(ushort.MaxValue, new Random(1), RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(413, 414, rSeq, RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(413, 414, new Random(1), RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(413, 413, rSeq, RandomGen.NextUInt32, "NextUInt32");
                            good &= _testResult<uint>(413, 413, new Random(1), RandomGen.NextUInt32, "NextUInt32");

                            good &= _testResult<long>(400, int.MaxValue + 1L, rSeq.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue + 1L, rSeq.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue, uint.MaxValue + 1L, rSeq.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue + 2L, rSeq.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue, uint.MaxValue, rSeq.Next64, "Next64");

                            good &= _testResult<long>(400, int.MaxValue + 1L, rSeq, RandomGen.Next64, "Next64");
                            good &= _testResult<long>(400, int.MaxValue + 1L, new Random(1), RandomGen.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue + 1L, rSeq, RandomGen.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue + 1L, new Random(1), RandomGen.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue, uint.MaxValue + 1L, rSeq, RandomGen.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue, uint.MaxValue + 1L, new Random(1), RandomGen.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue + 2L, rSeq, RandomGen.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue + 2L, new Random(1), RandomGen.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue, uint.MaxValue, rSeq, RandomGen.Next64, "Next64");
                            good &= _testResult<long>(uint.MaxValue, uint.MaxValue, new Random(1), RandomGen.Next64, "Next64");
                            good &= _testResult<long>(long.MaxValue - 2L, long.MaxValue, rSeq, RandomGen.Next64, "Next64");
                            good &= _testResult<long>(long.MaxValue - 2L, long.MaxValue, new Random(1), RandomGen.Next64, "Next64");
                            good &= _testResult<long>(0, long.MaxValue, rSeq, RandomGen.Next64, "Next64");
                            good &= _testResult<long>(0, long.MaxValue, new Random(1), RandomGen.Next64, "Next64");
                            good &= _testResult<long>(long.MaxValue, long.MaxValue, rSeq, RandomGen.Next64, "Next64");
                            good &= _testResult<long>(long.MaxValue, long.MaxValue, new Random(1), RandomGen.Next64, "Next64");

                            if (good) Console.WriteLine("All tests passed.");

                            Console.WriteLine();
                            Console.WriteLine("Testing whether Shuffle and ArrayShuffle give the same results ...");
                            int[] array1 = Iteration().ToArray();
                            int[] array2 = array1;
                            Console.WriteLine("Original:     " + string.Join<int>(", ", array1));
                            Random random1 = new Random(1);
                            Random random2 = new Random(1);
                            array1 = RandomGen.Shuffle<int>(random1, array1);
                            Console.WriteLine("Shuffle:      " + string.Join<int>(", ", array1));
                            RandomGen.ShuffleArray<int>(random2, array2);
                            Console.WriteLine("ArrayShuffle: " + string.Join<int>(", ", array2));
                            List<int> errors = new List<int>(array1.Length);
                            for (int i = 0; i < array1.Length; i++)
                            {
                                if (array1[i] != array2[i])
                                    errors.Add(i);
                            }
                            if (errors.Count == 0)
                                Console.WriteLine("Both shuffle methods are equivalent!");
                            else
                                Console.WriteLine("Failed starting at index " + errors.Count);
                            Console.WriteLine();
                        }
                        break;
                    default:
                        generator = null;
                        break;
                }

                if (generator != null)
                {
                    Console.Write("Generating ... ");

                    uint[] results = new uint[_maxVals];

                    uint min = uint.MaxValue, max = 0;

                    for (int i = 0; i < _maxVals; i++)
                    {
                        uint curVal = results[i] = RandomGen.NextUInt32(generator);
                        min = Math.Min(curVal, min);
                        max = Math.Max(curVal, max);
                    }

                    double avg = results.Average(i => i);

                    Console.WriteLine("done.");

                    const uint maxEq1 = uint.MaxValue;
                    const uint baseMedian = maxEq1 / 2;
                    const double maxInt = maxEq1;

                    Bitmap bmpBitmap = null, bmpGraphs = null, bmpGraphsBig = null;

                    string pathBitmap = generator.GetType().Name + ".Bitmap.png";
                    string pathGraphs = generator.GetType().Name + ".Graphs.png";
                    string pathGraphsBig = generator.GetType().Name + ".GraphsBig.png";

                    double maxDelta = (maxEq1 - max) / maxInt;
                    double avgDelta = Math.Abs(baseMedian - avg) / maxInt;
                    double minDelta = min / maxInt;

                    do
                    {
                        Console.WriteLine(generator.GetType().FullName);
                        Console.WriteLine("Expected maximum: 1.0");
                        Console.WriteLine("Actual maximum:   {0:F5} (delta: {1:F5}, {2:F5}%)", max / maxInt, maxDelta, (100f * maxDelta));
                        Console.WriteLine("Expected minimum: 0");
                        Console.WriteLine("Actual minimum:   {0:F5} (delta: {1:F5}, {2:F5}%)", minDelta, minDelta, (100f * minDelta));
                        Console.WriteLine("Expected average: 0.5");
                        Console.WriteLine("Actual average:   {0:F5} (delta: {1:F5}, {2:F5}%)", avg / maxInt, avgDelta, (100 * avgDelta));
                        Console.WriteLine();

                        Console.WriteLine("1. Bitmap (saved in working dir as {0})", pathBitmap);
                        Console.WriteLine("2. Graphs, Scatterplot, and Histograms for first 1000 (saved in working dir as {0})", pathGraphs);
                        Console.WriteLine("3. Graphs, Scatterplot, and Histograms for All " + _maxVals + " (saved in working dir as {0})", pathGraphsBig);
                        Console.WriteLine("4. Shuffle test");
                        Console.WriteLine("X. Return");
                        key = ReadKey();

                        switch (key)
                        {
                            case ConsoleKey.D1:
                                if (bmpBitmap == null || !File.Exists(pathBitmap))
                                {
                                    if (bmpBitmap == null)
                                    {
                                        const PixelFormat pixelFormat = PixelFormat.Format1bppIndexed;
                                        bmpBitmap = new Bitmap(_imgSize, _imgSize, pixelFormat);

                                        var bitLock = bmpBitmap.LockBits(new Rectangle(Point.Empty, bmpBitmap.Size), ImageLockMode.WriteOnly, pixelFormat);

                                        unsafe
                                        {
                                            uint* pStart = (uint*)bitLock.Scan0;

                                            for (int i = 0; i < _maxVals; i++)
                                                pStart[i] = results[i];
                                        }
                                    }
                                    bmpBitmap.Save(pathBitmap, ImageFormat.Png);
                                }
                                System.Diagnostics.Process.Start(pathBitmap);

                                break;
                            case ConsoleKey.D2:
                                buildGraph(results.Take(1000), pathGraphs, ref bmpGraphs);
                                break;
                            case ConsoleKey.D3:
                                buildGraph(results, pathGraphsBig, ref bmpGraphsBig);
                                break;
                            case ConsoleKey.D4:
                                {
                                    IEnumerable<int> iteration = Iteration();
                                    Console.WriteLine("Initial value: " + string.Join<int>(", ", iteration));
                                    Console.WriteLine("Shuffled value: " + string.Join<int>(", ", RandomGen.Shuffle(generator, iteration)));
                                    Console.WriteLine();
                                }
                                break;
                        }
                    }
                    while (key != ConsoleKey.X);
                    key = 0;

                    if (bmpBitmap != null)
                        bmpBitmap.Dispose();
                }

            }
            while (key != ConsoleKey.X);
        }

        private static IEnumerable<int> Iteration()
        {
            for (int i = 0; i < 30; i++)
                yield return i;
        }

        private static bool _testResult<T>(T min, T max, Func<T> genRand)
            where T : struct, IComparable<T>
        {
            bool returner = true;
            for (int i = 0; i < RandomSequence.SeedCount; i++)
            {
                T result = genRand();
                if (result.CompareTo(min) < 0 || result.CompareTo(max) > 0)
                {
                    Console.Error.WriteLine(" Error: result #{0} is out of the range! (inc.min:{1}, exc.max:{2}, value: {3})", i, min, max, result);
                    returner = false;
                }
            }
            return returner;
        }

        private static bool _testResult<T>(T min, T max, Func<T, T, T> genRand, string methodName)
            where T : struct, IComparable<T>
        {
            Console.WriteLine("RandomGen.{0}({1}, {2});", methodName, min, max);
            return _testResult<T>(min, max, () => genRand(min, max));
        }

        private static bool _testResult<T>(T max, Func<T, T> genRand, string methodName)
            where T : struct, IComparable<T>
        {
            Console.WriteLine("RandomGen.{0}({1});", methodName, max);
            return _testResult<T>(default(T), max, () => genRand(max));
        }

        private static bool _testResult<T>(T min, T max, Random generator, Func<Random, T, T, T> genRand, string methodName)
            where T : struct, IComparable<T>
        {
            Console.WriteLine("static RandomGen.{0}({1}, {2}, {3});", methodName, generator is RandomGen ? "RandomGen" : "Random", min, max);
            return _testResult<T>(min, max, () => genRand(generator, min, max));
        }

        private static bool _testResult<T>(T max, Random generator, Func<Random, T, T> genRand, string methodName)
            where T : struct, IComparable<T>
        {
            Console.WriteLine("static RandomGen.{0}({1}, {2});", methodName, generator is RandomGen ? "RandomGen" : "Random", max);
            return _testResult<T>(default(T), max, () => genRand(generator, max));
        }

        private static ConsoleKey ReadKey()
        {
            Console.Write("> ");
            ConsoleKeyInfo info = Console.ReadKey();

            Console.WriteLine(Environment.NewLine);
            return info.Key;
        }

        private static void buildGraph(IEnumerable<uint> resultCollect, string path, ref Bitmap bmp)
        {
            if (bmp == null || !File.Exists(path))
            {
                if (bmp == null)
                {
                    const long maxDiv = uint.MaxValue + 1L;
                    uint[] results = resultCollect is uint[] ? (uint[])resultCollect : resultCollect.ToArray();

                    const long histLength = 10;
                    int[] histogram = new int[histLength];
                    int histExpected = results.Length / (int)histLength;

                    foreach (uint curVal in results)
                        histogram[curVal * histLength / maxDiv]++;

                    const PixelFormat pixelFormat = PixelFormat.Format24bppRgb;
                    const int margin = 16;
                    const int leftMargin = margin * 3;
                    const int totalMargin = margin * 4;
                    int width = totalMargin + results.Length;

                    bmp = new Bitmap(width, 422 + leftMargin, pixelFormat);

                    using (Graphics g = Graphics.FromImage(bmp))
                    using (Font f = new Font(FontFamily.GenericMonospace, 7, FontStyle.Regular))
                    {
                        const int leftBorderX = leftMargin - 2;
                        const int firstTopY = margin, firstBottomY = firstTopY + 102;
                        const int secondTopY = firstBottomY + margin, secondBottomY = secondTopY + 102;
                        const int thirdTopY = secondBottomY + margin;
                        int rightBorderX = leftMargin + results.Length;

                        g.FillRectangle(Brushes.White, new Rectangle(Point.Empty, bmp.Size));

                        var hundreds = results.Select(i => (int)(i * 101L / maxDiv));

                        int? prevVal = null;
                        int curDex = leftMargin + 1;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        foreach (int curVal in hundreds)
                        {
                            if (prevVal != null)
                            {
                                int nextDex = curDex + 1;
                                g.DrawLine(Pens.Blue, new Point(curDex, firstTopY + prevVal.Value),
                                    new Point(nextDex, firstTopY + curVal));
                                curDex = nextDex;
                            }
                            prevVal = curVal;
                        }
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                        int prevDex = leftMargin;
                        curDex = leftMargin + 1;
                        foreach (int curVal in hundreds)
                        {
                            int nextDex = curDex + 1;

                            g.FillRectangle(Brushes.Blue, new Rectangle(prevDex, curVal + firstTopY - 1, 3, 3));
                            g.FillRectangle(Brushes.Blue, new Rectangle(prevDex, curVal + secondTopY - 1, 3, 3));

                            prevDex = curDex;
                            curDex = nextDex;
                        }
                        prevDex = leftMargin;
                        curDex = leftMargin + 1;
                        foreach (int curVal in hundreds)
                        {
                            int nextDex = curDex + 1;

                            g.FillRectangle(Brushes.Cyan, new Rectangle(curDex, curVal + firstTopY, 1, 1));
                            g.FillRectangle(Brushes.Cyan, new Rectangle(curDex, curVal + secondTopY, 1, 1));

                            prevDex = curDex;
                            curDex = nextDex;
                        }
                        for (int i = 0; i < histogram.Length; i++)
                        {
                            var curVal = histogram[i];
                            const double histOffset = 1.0 / histLength;
                            const int lineHeight = 200 / (int)histLength;

                            int diff = Math.Abs(histExpected - curVal);

                            int y = (lineHeight * i) + thirdTopY;
                            g.FillRectangle(Brushes.Cyan, new Rectangle(leftBorderX + 1, y, curVal, lineHeight));
                            g.FillRectangle(Brushes.DeepSkyBlue, new Rectangle(leftBorderX + 1, y, diff, lineHeight));
                            g.DrawRectangle(Pens.Black, new Rectangle(leftBorderX, y, curVal + 1, lineHeight));

                            string sRange = string.Format("{0:F1}-{1:F1}", histOffset * i, histOffset * (i + 1));
                            int textY = (lineHeight * i) + (thirdTopY + 1);
                            g.DrawString(sRange, f, Brushes.Black, leftBorderX - g.MeasureString(sRange, f).Width, textY);


                            string sValue = string.Format("{0} (DELTA: {1}", curVal, diff);

                            if (histExpected != 100)
                                sValue += ", " + (diff * 100F / histExpected);
                            sValue += "%)";

                            g.DrawString(sValue, f, Brushes.Black, (leftBorderX + 1) + curVal, textY);
                        }

                        g.DrawLine(Pens.Black, new Point(leftBorderX, firstTopY), new Point(leftBorderX, firstBottomY));
                        g.DrawLine(Pens.Black, new Point(leftBorderX, firstBottomY), new Point(rightBorderX, firstBottomY));

                        g.DrawLine(Pens.Black, new Point(leftBorderX, secondTopY), new Point(leftBorderX, secondBottomY));
                        g.DrawLine(Pens.Black, new Point(leftBorderX, secondBottomY), new Point(rightBorderX, secondBottomY));

                        g.DrawString("EXPECTED: " + histExpected, f, Brushes.Black, 1, secondBottomY + (margin / 3));
                        for (int i = 0; i < 10; i++)
                        {
                            int y = (i * 10);

                            const int leftBorderBegin = leftBorderX - 5;

                            int y1 = y + firstTopY;
                            int y2 = y + secondTopY;

                            g.DrawLine(Pens.Black, new Point(leftBorderBegin, y1), new Point(leftBorderX, y1));
                            g.DrawLine(Pens.Black, new Point(leftBorderBegin, y2), new Point(leftBorderX, y2));

                            if ((i & 1) == 0)
                            {
                                string s = ((10 - i) * 0.1).ToString("F1");

                                float xStr = leftBorderBegin - g.MeasureString(s, f).Width;

                                g.DrawString(s, f, Brushes.Black, xStr, y1 - 5);
                                g.DrawString(s, f, Brushes.Black, xStr, y2 - 5);
                            }
                        }
                    }
                }
                bmp.Save(path, ImageFormat.Png);
            }
            System.Diagnostics.Process.Start(path);
        }
    }

    class RandomSequence : RandomGen
    {
        private static uint[] _seeds;

        static RandomSequence()
        {
            _seeds = new uint[SeedCount];
            for (int i = 0; i < (SeedCount / 2); i++)
            {
                uint curVal = 1u << i;
                if (i > 0) _seeds[i * 2] = curVal;
                _seeds[(i * 2) + 1] = curVal | (curVal - 1);
            }
        }

        public const int SeedCount = 64;
        private const int _seedMask = SeedCount - 1;
        private int _curIndex = SeedCount;

        protected override uint SampleUInt32()
        {
            return _seeds[_curIndex = (_curIndex + 1) & _seedMask];
        }
    }
}
