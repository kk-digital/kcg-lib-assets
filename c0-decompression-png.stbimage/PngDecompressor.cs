using StbImageSharp;

namespace c0_decompression_png;

// PNG decompression result
public struct PngDecompressResult
{
    public byte[] PixelData;
    public int Width;
    public int Height;
    public int Components;
    public bool Success;
    public string Error;
}

// PNG decompression using StbImageSharp
public static class PngDecompressor
{
    public static PngDecompressResult Decompress(byte[] pngData)
    {
        try
        {
            using var stream = new MemoryStream(pngData);
            var result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            return new PngDecompressResult
            {
                PixelData = result.Data,
                Width = result.Width,
                Height = result.Height,
                Components = 4,
                Success = true,
                Error = null
            };
        }
        catch (Exception ex)
        {
            return new PngDecompressResult
            {
                PixelData = null,
                Width = 0,
                Height = 0,
                Components = 0,
                Success = false,
                Error = ex.Message
            };
        }
    }

    public static PngDecompressResult DecompressFromStream(Stream stream)
    {
        try
        {
            var result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            return new PngDecompressResult
            {
                PixelData = result.Data,
                Width = result.Width,
                Height = result.Height,
                Components = 4,
                Success = true,
                Error = null
            };
        }
        catch (Exception ex)
        {
            return new PngDecompressResult
            {
                PixelData = null,
                Width = 0,
                Height = 0,
                Components = 0,
                Success = false,
                Error = ex.Message
            };
        }
    }
}
