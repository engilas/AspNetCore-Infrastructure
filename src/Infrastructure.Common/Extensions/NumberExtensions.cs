using System;

namespace Infrastructure.Extensions
{
    public static class NumberExtensions
    {
        /// <summary>
        ///     Check that given number in range,
        /// </summary>
        /// <param name="number"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool In(this int number, int from, int to)
        {
            if (from >= to) throw new ArgumentException("from must be less than to");
            return number >= from && number <= to;
        }

        /// <summary>
        ///     Check that given number in range,
        /// </summary>
        /// <param name="number"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool In(this int number, (int from, int to) range)
        {
            return number.In(range.from, range.to);
        }
    }
}