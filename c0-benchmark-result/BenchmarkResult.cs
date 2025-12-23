namespace c0_benchmark_result;

// BENCHMARK DATA COLLECTION ARCHITECTURE
// =======================================
// CRITICAL: Do NOT contaminate benchmark timing data!
//
// PHASE 1: Data Collection (timed operations only)
// - File loading: Time file read from disk
// - Decompression: Time library decompression call
// - Store raw tick values immediately after each operation
// - NO sorting, NO statistics, NO computation during this phase
//
// PHASE 2: Post-Processing (after all timing complete)
// - Array sorting, grouping by size category
// - Statistics computation (mean, median, percentiles)
// - Summary aggregation, throughput calculations
// - Report generation
//
// This separation ensures timing data reflects actual operation time,
// not overhead from sorting, statistics, or other computations.

// Stage timing for benchmark phases
public struct StageTiming
{
    public string StageName;
    public long StartTicks;
    public long EndTicks;

    public long ElapsedTicks => EndTicks - StartTicks;
}

// Init timing - captured once per benchmark run
public struct InitTiming
{
    public StageTiming LibraryInit;
    public string LibraryName;
    public string LibraryVersion;
}

// Image size category
public struct SizeCategory
{
    public int ExactWidth;
    public int ExactHeight;
    public string RangeLabel; // "32", "64", "128", "256", "512", "1024", ">1024"

    public static string GetRangeLabel(int width, int height)
    {
        int maxDim = Math.Max(width, height);
        if (maxDim <= 32) return "32";
        if (maxDim <= 64) return "64";
        if (maxDim <= 128) return "128";
        if (maxDim <= 256) return "256";
        if (maxDim <= 512) return "512";
        if (maxDim <= 1024) return "1024";
        return ">1024";
    }
}

// Single file benchmark result
public struct FileBenchmarkResult
{
    public string FilePath;
    public string FileName;

    // Size info
    public int Width;
    public int Height;
    public SizeCategory Size;

    // Byte counts
    public long BytesIn;
    public long BytesOut;

    // Stage timings
    public StageTiming FileReadStage;
    public StageTiming DecompressionStage;

    // Status
    public bool Success;
    public string Error;

    // Library info
    public string LibraryName;
}

// Aggregate results for a size category
public struct SizeCategorySummary
{
    public string RangeLabel;
    public int FileCount;
    public string LibraryName;

    // Aggregated byte counts
    public long TotalBytesIn;
    public long TotalBytesOut;

    // Aggregated timings
    public long TotalReadTicks;
    public long TotalDecompressTicks;
}

// Complete benchmark run summary
public struct BenchmarkRunSummary
{
    public string Format; // "PNG" or "JPG"
    public string LibraryName;
    public InitTiming InitTiming;

    // Aggregated totals
    public int TotalFiles;
    public int SuccessCount;
    public int FailureCount;

    public long TotalBytesIn;
    public long TotalBytesOut;
    public long TotalReadTicks;
    public long TotalDecompressTicks;

    // Breakdown by size
    public SizeCategorySummary[] SizeBreakdown;
}
