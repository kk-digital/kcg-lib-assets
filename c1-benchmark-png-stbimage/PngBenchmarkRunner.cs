using c0_timing;
using c0_benchmark_result;
using c0_decompression_png;

namespace c1_benchmark_png_stbimage;

// PNG benchmark runner using StbImageSharp
public static class PngBenchmarkRunner
{
    public const string LibraryName = "StbImageSharp";
    public const string LibraryVersion = "2.30.15";

    public static InitTiming Initialize()
    {
        var initStart = IntervalTimer.GetTimestamp();
        // StbImageSharp has no explicit init, first decode warms up the library
        var initEnd = IntervalTimer.GetTimestamp();

        return new InitTiming
        {
            LibraryInit = new StageTiming
            {
                StageName = "LibraryInit",
                StartTicks = initStart,
                EndTicks = initEnd
            },
            LibraryName = LibraryName,
            LibraryVersion = LibraryVersion
        };
    }

    public static FileBenchmarkResult BenchmarkFile(string filePath)
    {
        var result = new FileBenchmarkResult
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            LibraryName = LibraryName
        };

        // Time file read
        var readStart = IntervalTimer.GetTimestamp();
        byte[] fileData;
        try
        {
            fileData = File.ReadAllBytes(filePath);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = $"Read error: {ex.Message}";
            return result;
        }
        var readEnd = IntervalTimer.GetTimestamp();

        result.BytesIn = fileData.Length;
        result.FileReadStage = new StageTiming
        {
            StageName = "FileRead",
            StartTicks = readStart,
            EndTicks = readEnd
        };

        // Time decompression
        var decompStart = IntervalTimer.GetTimestamp();
        var decompResult = PngDecompressor.Decompress(fileData);
        var decompEnd = IntervalTimer.GetTimestamp();

        result.DecompressionStage = new StageTiming
        {
            StageName = "Decompression",
            StartTicks = decompStart,
            EndTicks = decompEnd
        };

        if (decompResult.Success)
        {
            result.Success = true;
            result.BytesOut = decompResult.PixelData.Length;
            result.Width = decompResult.Width;
            result.Height = decompResult.Height;
            result.Size = new SizeCategory
            {
                ExactWidth = decompResult.Width,
                ExactHeight = decompResult.Height,
                RangeLabel = SizeCategory.GetRangeLabel(decompResult.Width, decompResult.Height)
            };
        }
        else
        {
            result.Success = false;
            result.Error = decompResult.Error;
        }

        return result;
    }

    public static List<FileBenchmarkResult> BenchmarkDirectory(string directory, Action<int, int> progressCallback = null)
    {
        var files = Directory.GetFiles(directory, "*.png");
        var results = new List<FileBenchmarkResult>(files.Length);

        for (int i = 0; i < files.Length; i++)
        {
            results.Add(BenchmarkFile(files[i]));
            progressCallback?.Invoke(i + 1, files.Length);
        }

        return results;
    }

    public static BenchmarkRunSummary CreateSummary(InitTiming initTiming, List<FileBenchmarkResult> results)
    {
        var successful = results.Where(r => r.Success).ToList();
        var sizeCategories = new[] { "32", "64", "128", "256", "512", "1024", ">1024" };

        var sizeBreakdown = sizeCategories
            .Select(cat =>
            {
                var catResults = successful.Where(r => r.Size.RangeLabel == cat).ToList();
                return new SizeCategorySummary
                {
                    RangeLabel = cat,
                    FileCount = catResults.Count,
                    LibraryName = LibraryName,
                    TotalBytesIn = catResults.Sum(r => r.BytesIn),
                    TotalBytesOut = catResults.Sum(r => r.BytesOut),
                    TotalReadTicks = catResults.Sum(r => r.FileReadStage.ElapsedTicks),
                    TotalDecompressTicks = catResults.Sum(r => r.DecompressionStage.ElapsedTicks)
                };
            })
            .Where(s => s.FileCount > 0)
            .ToArray();

        return new BenchmarkRunSummary
        {
            Format = "PNG",
            LibraryName = LibraryName,
            InitTiming = initTiming,
            TotalFiles = results.Count,
            SuccessCount = successful.Count,
            FailureCount = results.Count - successful.Count,
            TotalBytesIn = successful.Sum(r => r.BytesIn),
            TotalBytesOut = successful.Sum(r => r.BytesOut),
            TotalReadTicks = successful.Sum(r => r.FileReadStage.ElapsedTicks),
            TotalDecompressTicks = successful.Sum(r => r.DecompressionStage.ElapsedTicks),
            SizeBreakdown = sizeBreakdown
        };
    }
}
