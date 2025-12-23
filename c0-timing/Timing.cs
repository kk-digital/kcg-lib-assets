using System.Diagnostics;

namespace c0_timing;

// TIMING LIBRARY
// ==============
// Provides high-precision interval timing for benchmarking.
// Uses Stopwatch for nanosecond-level precision.
// This library should contain ONLY timing-related code.
// Benchmark result structs are in c0-benchmark-result.

// Interval timing with nanosecond precision
public struct TimingInterval
{
    public long StartTicks;
    public long EndTicks;
    public string Name;

    public long ElapsedTicks => EndTicks - StartTicks;
    public long ElapsedNanoseconds => (ElapsedTicks * 1_000_000_000L) / Stopwatch.Frequency;
    public double ElapsedMilliseconds => (double)ElapsedTicks / Stopwatch.Frequency * 1000.0;
    public double ElapsedSeconds => (double)ElapsedTicks / Stopwatch.Frequency;
}

// Stopwatch wrapper for interval timing
public static class IntervalTimer
{
    // Get current high-resolution timestamp
    public static long GetTimestamp()
    {
        return Stopwatch.GetTimestamp();
    }

    // Convert ticks to nanoseconds
    public static long TicksToNanoseconds(long ticks)
    {
        return (ticks * 1_000_000_000L) / Stopwatch.Frequency;
    }

    // Convert ticks to milliseconds
    public static double TicksToMilliseconds(long ticks)
    {
        return (double)ticks / Stopwatch.Frequency * 1000.0;
    }

    // Convert ticks to seconds
    public static double TicksToSeconds(long ticks)
    {
        return (double)ticks / Stopwatch.Frequency;
    }

    // Create interval from start/end ticks
    public static TimingInterval CreateInterval(string name, long startTicks, long endTicks)
    {
        return new TimingInterval
        {
            Name = name,
            StartTicks = startTicks,
            EndTicks = endTicks
        };
    }

    // Get Stopwatch frequency (ticks per second)
    public static long Frequency => Stopwatch.Frequency;
}
