namespace c0_benchmark_result;

// ARCHITECTURE NOTES:
// ===================
// 1. All decompression libraries receive file data as byte[] - they DO NOT read from disk
// 2. File loading is handled by the benchmark runner to:
//    - Standardize file I/O timing across all libraries
//    - Ensure consistent file reading behavior
//    - Allow proper benchmarking of file I/O vs decompression
// 3. All libraries output ImageOutput with RGBA pixel data
// 4. Output can be verified byte-for-byte against expected output

// Standard file input - passed to all decompression libraries
public struct FileInput
{
    // File metadata
    public string FileName;       // Original filename without path
    public string FilePath;       // Full path to file on disk
    public long FileSize;         // Size in bytes of original file

    // File content - loaded by benchmark runner, passed to library
    public byte[] Data;           // Raw file bytes (PNG/JPG format)

    // ARCHITECTURE NOTE:
    // Libraries must NOT read from disk themselves.
    // The benchmark runner loads the file and passes Data to the library.
    // This ensures:
    // - Consistent file loading timing
    // - Library only does decompression work
    // - File I/O can be timed separately from decompression
}

// Standard image output - returned by all decompression libraries
public struct ImageOutput
{
    // Image dimensions
    public int Width;             // Image width in pixels
    public int Height;            // Image height in pixels
    public int Components;        // Number of color components (4 for RGBA)

    // Pixel data - RGBA format, 4 bytes per pixel
    public byte[] PixelData;      // Raw pixel data: [R,G,B,A, R,G,B,A, ...]
                                  // Size = Width * Height * 4 bytes

    // Status
    public bool Success;          // True if decompression succeeded
    public string Error;          // Error message if failed

    // ARCHITECTURE NOTE:
    // All libraries output RGBA format (4 components per pixel).
    // This allows byte-for-byte comparison of output across libraries.
    // PixelData layout: Row-major order, top-to-bottom, left-to-right.
    // Byte order per pixel: Red, Green, Blue, Alpha (RGBA).

    public long PixelDataSize => Width * Height * Components;
}

// Standard verification output - for comparing library outputs
public struct VerificationOutput
{
    public FileInput Input;       // Original input
    public ImageOutput Output;    // Library output
    public string LibraryName;    // Which library produced this

    // Verification data
    public byte[] ExpectedPixels; // Expected pixel data (if available)
    public bool MatchesExpected;  // True if PixelData matches ExpectedPixels

    // ARCHITECTURE NOTE:
    // Used to verify that different libraries produce identical output.
    // ExpectedPixels can be loaded from a reference file or from another library.
    // Comparison is byte-for-byte exact match.
}

// LIBRARY INTEGRATION NOTES:
// ==========================
// Each decompression library wrapper must:
// 1. Accept FileInput.Data as input (not file path)
// 2. Return ImageOutput with RGBA pixel data
// 3. Set Success=true on success, Success=false on failure
// 4. Fill Error message on failure
// 5. Set Width, Height, Components correctly
//
// Example usage:
//   FileInput input = LoadFile("image.png");
//   ImageOutput output = PngDecompressor.Decompress(input.Data);
//   if (output.Success) { /* use output.PixelData */ }
