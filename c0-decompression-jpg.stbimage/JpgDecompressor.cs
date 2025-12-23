using StbImageSharp;

namespace c0_decompression_jpg;

// JPG decompression result
public struct JpgDecompressResult
{
    public byte[] PixelData;
    public int Width;
    public int Height;
    public int Components;
    public bool Success;
    public string Error;
}

// JPG decompression using StbImageSharp
public static class JpgDecompressor
{
    public static JpgDecompressResult Decompress(byte[] jpgData)
    {
        try
        {
            using var stream = new MemoryStream(jpgData);
            var result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            return new JpgDecompressResult
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
            return new JpgDecompressResult
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

    public static JpgDecompressResult DecompressFromStream(Stream stream)
    {
        try
        {
            var result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            return new JpgDecompressResult
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
            return new JpgDecompressResult
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
