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
#if !NOCONTRACT
using System.Diagnostics.Contracts;
#endif
#if !NOLINQ
using System.Linq;
#endif
#if !NOBIGINT
using System.Numerics;
#endif

namespace URandomGen
{
    /// <summary>
    /// Base class for random number generators which use this schema; includes utility methods used by all classes.
    /// </summary>
    public abstract class RandomGen : Random
    {
        /// <summary>
        /// Returns an array containing a set of default seed values.
        /// </summary>
        /// <returns>An array containing four elements: <see cref="Environment.TickCount"/>, the upper 32 bits of the <see cref="DateTime.Ticks"/>
        /// property of <see cref="DateTime.Now"/>, the lower 32 bits thereof, and the sum of all previous values, all converted to <see cref="UInt32"/>
        /// values.</returns>
        public static uint[] DefaultSeeds()
        {
            uint tickCount = (uint)Environment.TickCount;
            ulong nowTicks = (ulong)DateTime.Now.Ticks;

            uint[] seeds = new uint[]
            {
                tickCount,
                (uint)(nowTicks >> 32),
                (uint)nowTicks,
                0
            };

            seeds[3] = seeds[0] + seeds[1] + seeds[2];

            return seeds;
        }

        /// <summary>
        /// Returns a single default seed.
        /// </summary>
        /// <returns>The lower 32 bits of the <see cref="DateTime.Ticks"/> property of <see cref="DateTime.Now"/>.</returns>
        public static uint DefaultSingleSeed()
        {
            return (uint)DateTime.Now.Ticks;
        }

        internal static bool IsNextThreeZero(uint[] seeds, int curIndex)
        {
#if NOLINQ
            int count = 0;
            foreach (uint curItem in ArrayOffset(seeds, curIndex))
            {
                if (curItem != 0) return false;

                if (++count >= 3) break;
            }
            return true;
#else
            using (IEnumerator<uint> enumerator = ArrayOffset(seeds, curIndex).Take(3).Where(i => i != 0).GetEnumerator())
                return !enumerator.MoveNext();
#endif
        }
#if NOLINQ
        internal static T[] ToArray<T>(IEnumerable<T> collection)
        {
            if (collection is ICollection<T>)
            {
                T[] array = new T[((ICollection<T>)collection).Count];
                ((ICollection<T>)collection).CopyTo(array, 0);
                return array;
            }
            if (collection is System.Collections.ICollection)
            {
                T[] array = new T[((System.Collections.ICollection)collection).Count];
                ((System.Collections.ICollection)collection).CopyTo(array, 0);
                return array;
            }

            T[] buffer = new T[16];
            int curCount = 0;
            foreach (T item in collection)
            {
                if (curCount >= buffer.Length)
                    Array.Resize<T>(ref buffer, checked(buffer.Length * 2));

                buffer[curCount++] = item;
            }

            if (buffer.Length != curCount)
                Array.Resize(ref buffer, curCount);

            return buffer;
        }
#endif
        internal static IEnumerable<T> ArrayOffset<T>(T[] array, int offset)
        {
            for (int i = offset; i < array.Length; i++)
                yield return array[i];
            for (int i = 0; i < offset; i++)
                yield return array[i];
        }

        /// <summary>
        /// When overridden in a derived class, this method is used by other methods to generate random 32-bit numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer which is greater than or equal to 0 and less than or equal to <see cref="UInt32.MaxValue"/>.</returns>
        protected abstract uint SampleUInt32();

        /// <summary>
        /// This method is used by other methods to generate random 64-bit numbers.
        /// </summary>
        /// <returns>A 32-bit unsigned integer which is greater than or equal to 0 and less than or equal to <see cref="UInt64.MaxValue"/>.</returns>
        protected virtual ulong SampleUInt64()
        {
            return ((ulong)SampleUInt32() << 32) | SampleUInt32();
        }

        /// <summary>
        /// Return a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A random number which is greater than or equal to 0.0 and which is less than 1.0.</returns>
        protected sealed override double Sample()
        {
            const double max = max32;

            return SampleUInt32() / max;
        }

        private static long _sampleValue(RandomGen generator, long length)
        {
            return ((length * generator.SampleUInt32()) / max32);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 32-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                base.Next(minValue, maxValue); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<int>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<int>() < maxValue);
            Contract.EndContractBlock();
#endif
            long length = maxValue - (long)minValue;

            return (int)(minValue + _sampleValue(this, length));
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 32-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public override int Next(int maxValue)
        {
            if (maxValue < 0)
                base.Next(maxValue); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<int>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (int)_sampleValue(this, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A signed 32-bit integer which is greater than or equal to 0 and less than <see cref="Int32.MaxValue"/>.</returns>
        public override int Next()
        {
            return Next(int.MaxValue);
        }


        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public uint NextUInt32(uint minValue, uint maxValue)
        {
            if (minValue > maxValue)
                base.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<uint>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();
#endif
            long length = maxValue - (long)minValue;

            return (uint)(minValue + _sampleValue(this, length));
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public uint NextUInt32(uint maxValue)
        {
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<uint>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (uint)_sampleValue(this, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <see cref="Int32.MaxValue"/>.</returns>
        public uint NextUInt32()
        {
            return NextUInt32(uint.MaxValue);
        }

        private const int max16 = 1 << 16;
        private const long max32 = 1L << 32;
        private const long max48 = 1L << 48;
#if NOBIGINT
        private const decimal max64 = ulong.MaxValue + 1.0m;
#else
        private static readonly BigInteger max64 = new BigInteger(ulong.MaxValue) + 1;
#endif
        private static uint _nextUInt32(Random generator, uint minValue, uint maxValue)
        {
            long length = maxValue - (long)minValue;
            if (generator is RandomGen)
                return (uint)(minValue + _sampleValue((RandomGen)generator, length));

            if (length <= int.MaxValue)
                return (uint)(generator.Next((int)length) + minValue);

            uint result = ((uint)generator.Next(max16) << 16) | (uint)generator.Next(max16);

            return (uint)((length * result) / max32) + minValue;
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public static uint NextUInt32(Random generator, uint minValue, uint maxValue)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (minValue > maxValue)
                generator.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<uint>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();
#endif
            return _nextUInt32(generator, minValue, maxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public static uint NextUInt32(Random generator, uint maxValue)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (maxValue < 0)
                generator.Next(-1); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<uint>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<uint>() < maxValue);
            Contract.EndContractBlock();
#endif
            return _nextUInt32(generator, 0, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>An unsigned 32-bit integer which is greater than or equal to 0 and less than <see cref="UInt32.MaxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        public static uint NextUInt32(Random generator)
        {
            return NextUInt32(generator, uint.MaxValue);
        }

#if NOBIGINT
        private static decimal _sampleValue(RandomGen generator, decimal length)
#else
        private static BigInteger _sampleValue(RandomGen generator, BigInteger length)
#endif
        {
            if (length == max32)
                return generator.SampleUInt32();
            if (length <= max32)
                return (length * generator.SampleUInt32()) / max32;

#if NOBIGINT
            return (length / max64) * generator.SampleUInt64();
#else
            return (length * generator.SampleUInt64()) / max64;
#endif
        }

#if NOBIGINT
        private static decimal _next64(Random generator, decimal minValue, decimal maxValue)
        {
            decimal length = maxValue - minValue;
#else
        private static BigInteger _next64(Random generator, BigInteger minValue, BigInteger maxValue)
        {
            BigInteger length = maxValue - minValue;
#endif
            if (generator is RandomGen)
                return minValue + _sampleValue((RandomGen)generator, length);

            if (length <= int.MaxValue)
                return minValue + generator.Next((int)length);

            ulong result = (uint)generator.Next(max16) | ((uint)generator.Next(max16) << 16);

            if (length < max32)
                return (length * result) / max32;

            result |= (ulong)generator.Next(max16) << 32;

            if (length < max48)
#if NOBIGINT
                return (length / max48) * result;
#else
                return (length * result) / max48;
#endif

            result |= (ulong)generator.Next(max16) << 48;

#if NOBIGINT
            return ((length / max64) * result) + minValue;
#else
            return ((length * result) / max64) + minValue;
#endif
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public long Next64(long minValue, long maxValue)
        {
            if (minValue > maxValue)
                base.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<long>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();
#endif
#if NOBIGINT
            decimal length = (decimal)maxValue - minValue;
#else
            BigInteger length = (BigInteger)maxValue - minValue;
#endif
            return (long)(_sampleValue(this, length) + minValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public long Next64(long maxValue)
        {
            if (maxValue < 0)
                base.Next(-1); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<long>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (long)_sampleValue(this,
#if NOBIGINT
                (decimal)maxValue);
#else
                (BigInteger)maxValue);
#endif
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A signed 64-bit integer which is greater than or equal to 0 and less than <see cref="Int32.MaxValue"/>.</returns>
        public long Next64()
        {
            return Next64(long.MaxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public static long Next64(Random generator, long minValue, long maxValue)
        {
            if (minValue > maxValue)
                generator.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<long>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (long)_next64(generator, minValue, maxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public static long Next64(Random generator, long maxValue)
        {
            if (maxValue < 0)
                generator.Next(-1); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<long>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<long>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (long)_next64(generator, 0, maxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>A signed 64-bit integer which is greater than or equal to 0 and less than <see cref="Int64.MaxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        public static long Next64(Random generator)
        {
            return Next64(generator, long.MaxValue);
        }


        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public ulong NextUInt64(ulong minValue, ulong maxValue)
        {
            if (minValue > maxValue)
                base.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<ulong>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();
#endif
#if NOBIGINT
            decimal length = (decimal)maxValue - minValue;
#else
            BigInteger length = (BigInteger)maxValue - minValue;
#endif

            return (ulong)(_sampleValue(this, length) + minValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public ulong NextUInt64(ulong maxValue)
        {
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<ulong>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (ulong)_sampleValue(this, maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to 0 and less than <see cref="Int64.MaxValue"/>.</returns>
        public ulong NextUInt64()
        {
            return NextUInt64(ulong.MaxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="minValue">The inclusive lower bound of the random value.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than <paramref name="minValue"/>.
        /// </exception>
        public static ulong NextUInt64(Random generator, ulong minValue, ulong maxValue)
        {
            if (minValue > maxValue)
                generator.Next(1, 0); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<ulong>() >= minValue);
            Contract.Ensures(maxValue == minValue || Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (ulong)_next64(generator, minValue, maxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="maxValue">The exclusive upper bound of the random value.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to 0 and less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxValue"/> is less than 0.
        /// </exception>
        public static ulong NextUInt64(Random generator, ulong maxValue)
        {
            if (maxValue < 0)
                generator.Next(-1); //Throw ArgumentOutOfRangeException according to default form.
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<ulong>() >= 0);
            Contract.Ensures(maxValue == 0 || Contract.Result<ulong>() < maxValue);
            Contract.EndContractBlock();
#endif
            return (ulong)_next64(generator, 0, maxValue);
        }

        /// <summary>
        /// Returns a random integer within a specified range.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <returns>An unsigned 64-bit integer which is greater than or equal to 0 and less than <see cref="UInt64.MaxValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> is <c>null</c>.
        /// </exception>
        public static ulong NextUInt64(Random generator)
        {
            return NextUInt64(generator, ulong.MaxValue);
        }

        /// <summary>
        /// Fills the elements of the specified array of bytes with random numbers between 0 and 255 inclusive.
        /// </summary>
        /// <param name="buffer">A byte array to fill with random numbers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <c>null</c>.
        /// </exception>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            const int maxBytes = byte.MaxValue + 1;

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(SampleUInt32() % maxBytes);
        }

        /// <summary>
        /// Fills the elements of the specified array of bytes with random numbers between 1 and 255 inclusive.
        /// </summary>
        /// <param name="buffer">A byte array to fill with random numbers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <c>null</c>.
        /// </exception>
        public void NextNonZeroBytes(byte[] buffer)
        {
            NextNonZeroBytes(this, buffer);
        }

        /// <summary>
        /// Fills the elements of the specified array of bytes with random numbers between 1 and 255 inclusive.
        /// </summary>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="buffer">A byte array to fill with random numbers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is <c>null</c>.
        /// </exception>
        public static void NextNonZeroBytes(Random generator, byte[] buffer)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (buffer == null) throw new ArgumentNullException("buffer");
#if !NOCONTRACT
            Contract.Ensures(Array.IndexOf(Contract.ValueAtReturn(out buffer), 0) < 0);
            Contract.EndContractBlock();
#endif
            const int maxBytes = byte.MaxValue + 1;
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)generator.Next(1, maxBytes);
        }

        private static bool TryGetCount<T>(IEnumerable<T> collection, out int count)
        {
            if (collection is ICollection<T>)
            {
                count = ((ICollection<T>)collection).Count;
                return true;
            }
            if (collection is System.Collections.ICollection)
            {
                count = ((System.Collections.ICollection)collection).Count;
                return true;
            }
#if NET_4_5
            if (collection is IReadOnlyCollection<T>)
            {
                count = ((IReadOnlyCollection<T>)collection).Count;
                return true;
            }
#endif
            count = 0;
            return false;
        }

        private static void _shuffleArray<T>(Random generator, T[] array, int startIndex, int length)
        {
            int next;

            for (int i = 0, iPos = startIndex; i < length; i = next, iPos++)
            {
                next = i + 1;
                int j = i == 0 ? 0 : generator.Next(0, next);

                if (i == j)
                    continue;

                j += startIndex;

                T item = array[iPos];
                array[iPos] = array[j];
                array[j] = item;
            }
        }

        /// <summary>
        /// Returns all elements in the specified collection in random order.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="collection">The collection whose elements will be shuffled.</param>
        /// <returns>A list containing the shuffled elements in <paramref name="collection"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> or <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        public static T[] Shuffle<T>(Random generator, IEnumerable<T> collection)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (collection == null) throw new ArgumentNullException("collection");
#if !NOCONTRACT
            Contract.Ensures(Contract.Result<T[]>() != null);
            Contract.EndContractBlock();
#endif
            int count;
            if (TryGetCount<T>(collection, out count))
            {
                T[] list = new T[count];
                if (count == 0) return list;
                count = 0;

                foreach (T item in collection)
                {
                    int next = count + 1;
                    int j = count == 0 ? 0 : generator.Next(0, next);

                    if (j == count)
                    {
                        list[count] = item;
                        count = next;
                        continue;
                    }
                    list[count] = list[j];
                    list[j] = item;
                    count = next;
                }
                return list;
            }
#if NOLINQ
            T[] array = ToArray<T>(collection);
#else
            T[] array = collection.ToArray();
#endif
            _shuffleArray<T>(generator, array, 0, array.Length);
            return array;
        }

        /// <summary>
        /// Returns all elements in the specified collection in random order.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="collection">The collection whose elements will be shuffled.</param>
        /// <returns>A list containing the shuffled elements in <paramref name="collection"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        public T[] Shuffle<T>(IEnumerable<T> collection)
        {
            return Shuffle<T>(this, collection);
        }

        /// <summary>
        /// Shuffles all elements in the specified array.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="generator">The random number generator to use.</param>
        /// <param name="array">The array whose elements will be shuffled.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generator"/> or <paramref name="array"/> is <c>null</c>.
        /// </exception>
        public static void ShuffleArray<T>(Random generator, T[] array)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            if (array == null) throw new ArgumentNullException("array");
#if !NOCONTRACT
            Contract.EndContractBlock();
#endif
            _shuffleArray<T>(generator, array, 0, array.Length);
        }

        /// <summary>
        /// Shuffles all elements in the specified array.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="array">The array whose elements will be shuffled.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is <c>null</c>.
        /// </exception>
        public void ShuffleArray<T>(T[] array)
        {
            ShuffleArray<T>(this, array);
        }
    }
}
