using System.Text;
using c0_timing;
using c0_benchmark_result;
using c1_benchmark_png_stbimage;
using c1_benchmark_jpg_stbimage;

namespace c2_benchmark;

// BENCHMARK CLI APPLICATION
// =========================
// This is the main CLI runner for benchmarking.
// It uses library-specific runners (c1 layer) for actual benchmarking.
// All statistics and report generation happen AFTER data collection.
// Reports are saved to output/ folder.

public static class Program
{
    private static StringBuilder _reportBuilder = new StringBuilder();
    private static string _outputDir = "output";

    public static int Main(string[] args)
    {
        string dataDir = args.Length > 0 ? args[0] : "data";

        if (!Directory.Exists(dataDir))
        {
            Console.WriteLine($"Error: Data directory not found: {dataDir}");
            return 1;
        }

        // Create output directory if needed
        Directory.CreateDirectory(_outputDir);

        // Generate timestamp for report filename
        string timestamp = DateTime.Now.ToString("yyyy-MMdd-HHmmss");
        string reportPath = Path.Combine(_outputDir, $"benchmark-{timestamp}.txt");

        WriteLineToReport("=".PadRight(100, '='));
        WriteLineToReport("Image Decompression Benchmark");
        WriteLineToReport($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        WriteLineToReport("=".PadRight(100, '='));
        WriteLineToReport("");

        // Run PNG benchmark
        var pngDir = Path.Combine(dataDir, "png");
        if (Directory.Exists(pngDir))
        {
            RunPngBenchmark(pngDir);
        }
        else
        {
            WriteLineToReport($"PNG directory not found: {pngDir}");
        }

        WriteLineToReport("");

        // Run JPG benchmark
        var jpgDir = Path.Combine(dataDir, "jpg");
        if (Directory.Exists(jpgDir))
        {
            RunJpgBenchmark(jpgDir);
        }
        else
        {
            WriteLineToReport($"JPG directory not found: {jpgDir}");
        }

        // Save report to file
        File.WriteAllText(reportPath, _reportBuilder.ToString());
        Console.WriteLine();
        Console.WriteLine($"Report saved to: {reportPath}");

        return 0;
    }

    static void WriteLineToReport(string line)
    {
        Console.WriteLine(line);
        _reportBuilder.AppendLine(line);
    }

    static void RunPngBenchmark(string pngDir)
    {
        WriteLineToReport("-".PadRight(100, '-'));
        WriteLineToReport($"PNG Decompression Benchmark (Library: {PngBenchmarkRunner.LibraryName})");
        WriteLineToReport("-".PadRight(100, '-'));

        // PHASE 1: Initialize and time library init
        var initTiming = PngBenchmarkRunner.Initialize();

        // PHASE 2: Collect benchmark data (NO computation during this phase)
        var files = Directory.GetFiles(pngDir, "*.png");
        WriteLineToReport($"Files found: {files.Length}");
        WriteLineToReport("");

        int lastPrinted = 0;
        var results = PngBenchmarkRunner.BenchmarkDirectory(pngDir, (current, total) =>
        {
            if (current % 100 == 0 || current == total)
            {
                if (current != lastPrinted)
                {
                    Console.Write($"\rProcessed: {current} of {total} PNG files   ");
                    lastPrinted = current;
                }
            }
        });
        Console.WriteLine();
        _reportBuilder.AppendLine($"Processed: {results.Count} of {files.Length} PNG files");

        // PHASE 3: Post-processing - statistics and report generation
        // All computation happens AFTER data collection to avoid contaminating timing
        var summary = PngBenchmarkRunner.CreateSummary(initTiming, results);
        PrintTableReport(summary, results);
    }

    static void RunJpgBenchmark(string jpgDir)
    {
        WriteLineToReport("-".PadRight(100, '-'));
        WriteLineToReport($"JPG Decompression Benchmark (Library: {JpgBenchmarkRunner.LibraryName})");
        WriteLineToReport("-".PadRight(100, '-'));

        // PHASE 1: Initialize and time library init
        var initTiming = JpgBenchmarkRunner.Initialize();

        // PHASE 2: Collect benchmark data (NO computation during this phase)
        var files = Directory.GetFiles(jpgDir, "*.jpg");
        WriteLineToReport($"Files found: {files.Length}");
        WriteLineToReport("");

        int lastPrinted = 0;
        var results = JpgBenchmarkRunner.BenchmarkDirectory(jpgDir, (current, total) =>
        {
            if (current % 10 == 0 || current == total)
            {
                if (current != lastPrinted)
                {
                    Console.Write($"\rProcessed: {current} of {total} JPG files   ");
                    lastPrinted = current;
                }
            }
        });
        Console.WriteLine();
        _reportBuilder.AppendLine($"Processed: {results.Count} of {files.Length} JPG files");

        // PHASE 3: Post-processing - statistics and report generation
        var summary = JpgBenchmarkRunner.CreateSummary(initTiming, results);
        PrintTableReport(summary, results);
    }

    static void PrintTableReport(BenchmarkRunSummary summary, List<FileBenchmarkResult> results)
    {
        WriteLineToReport($"Results: {summary.SuccessCount} of {summary.TotalFiles} {summary.Format} files decompressed successfully");
        WriteLineToReport($"Library: {summary.LibraryName} (Init time: {FormatTime(IntervalTimer.TicksToNanoseconds(summary.InitTiming.LibraryInit.ElapsedTicks))})");
        WriteLineToReport("");

        // Print table header
        WriteLineToReport($"{"Size",-10} | {"Count",-6} | {"Library",-14} | {"Read Time",-12} | {"Decomp Time",-12} | {"In B/s",-12} | {"Out B/s",-12}");
        WriteLineToReport(new string('-', 100));

        // Print each size category
        foreach (var cat in summary.SizeBreakdown)
        {
            double readSeconds = IntervalTimer.TicksToSeconds(cat.TotalReadTicks);
            double decompSeconds = IntervalTimer.TicksToSeconds(cat.TotalDecompressTicks);

            double inBytesPerSec = decompSeconds > 0 ? cat.TotalBytesIn / decompSeconds : 0;
            double outBytesPerSec = decompSeconds > 0 ? cat.TotalBytesOut / decompSeconds : 0;

            string sizeLabel = cat.RangeLabel == ">1024" ? ">1024" : $"{cat.RangeLabel}x{cat.RangeLabel}";
            WriteLineToReport($"{sizeLabel,-10} | {cat.FileCount,-6} | {cat.LibraryName,-14} | {FormatTime(IntervalTimer.TicksToNanoseconds(cat.TotalReadTicks)),-12} | {FormatTime(IntervalTimer.TicksToNanoseconds(cat.TotalDecompressTicks)),-12} | {FormatBytesPerSecond(inBytesPerSec),-12} | {FormatBytesPerSecond(outBytesPerSec),-12}");
        }

        WriteLineToReport(new string('-', 100));

        // Print totals
        double grandReadSeconds = IntervalTimer.TicksToSeconds(summary.TotalReadTicks);
        double grandDecompSeconds = IntervalTimer.TicksToSeconds(summary.TotalDecompressTicks);

        double grandInBytesPerSec = grandDecompSeconds > 0 ? summary.TotalBytesIn / grandDecompSeconds : 0;
        double grandOutBytesPerSec = grandDecompSeconds > 0 ? summary.TotalBytesOut / grandDecompSeconds : 0;

        WriteLineToReport($"{"TOTAL",-10} | {summary.SuccessCount,-6} | {summary.LibraryName,-14} | {FormatTime(IntervalTimer.TicksToNanoseconds(summary.TotalReadTicks)),-12} | {FormatTime(IntervalTimer.TicksToNanoseconds(summary.TotalDecompressTicks)),-12} | {FormatBytesPerSecond(grandInBytesPerSec),-12} | {FormatBytesPerSecond(grandOutBytesPerSec),-12}");
        WriteLineToReport("");

        // Additional summary
        WriteLineToReport($"Total bytes in:  {FormatBytes(summary.TotalBytesIn)}");
        WriteLineToReport($"Total bytes out: {FormatBytes(summary.TotalBytesOut)}");
        if (summary.TotalBytesOut > 0)
            WriteLineToReport($"Compression ratio: {(double)summary.TotalBytesIn / summary.TotalBytesOut:F3}x");

        // Show failures if any
        var failures = results.Where(r => !r.Success).ToList();
        if (failures.Count > 0)
        {
            WriteLineToReport("");
            WriteLineToReport($"Failures ({failures.Count}):");
            foreach (var f in failures.Take(5))
            {
                WriteLineToReport($"  {f.FileName}: {f.Error}");
            }
            if (failures.Count > 5)
            {
                WriteLineToReport($"  ... and {failures.Count - 5} more");
            }
        }
    }

    static string FormatBytes(long bytes)
    {
        if (bytes >= 1_000_000_000)
            return $"{bytes / 1_000_000_000.0:F2} GB";
        if (bytes >= 1_000_000)
            return $"{bytes / 1_000_000.0:F2} MB";
        if (bytes >= 1_000)
            return $"{bytes / 1_000.0:F2} KB";
        return $"{bytes} B";
    }

    static string FormatBytesPerSecond(double bytesPerSecond)
    {
        if (double.IsNaN(bytesPerSecond) || double.IsInfinity(bytesPerSecond) || bytesPerSecond <= 0)
            return "N/A";
        if (bytesPerSecond >= 1_000_000_000)
            return $"{bytesPerSecond / 1_000_000_000.0:F2} GB/s";
        if (bytesPerSecond >= 1_000_000)
            return $"{bytesPerSecond / 1_000_000.0:F2} MB/s";
        if (bytesPerSecond >= 1_000)
            return $"{bytesPerSecond / 1_000.0:F2} KB/s";
        return $"{bytesPerSecond:F2} B/s";
    }

    static string FormatTime(long nanoseconds)
    {
        if (nanoseconds >= 1_000_000_000)
            return $"{nanoseconds / 1_000_000_000.0:F3} s";
        if (nanoseconds >= 1_000_000)
            return $"{nanoseconds / 1_000_000.0:F2} ms";
        if (nanoseconds >= 1_000)
            return $"{nanoseconds / 1_000.0:F2} us";
        return $"{nanoseconds} ns";
    }
}
