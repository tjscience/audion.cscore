namespace System
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Round(this TimeSpan ts, int precision)
        {
            if (precision < 0) { throw new ArgumentException("precision must be non-negative"); }
            int factor = (int)System.Math.Pow(10, (7 - precision));

            // This is only valid for rounding milliseconds-will *not* work on secs/mins/hrs!
            return new TimeSpan(((long)System.Math.Round((1.0 * ts.Ticks / factor)) * factor));
        }
    }
}
