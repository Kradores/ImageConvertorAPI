using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Net.Http.Headers;

public static class TestFileHelper
{
    public static ByteArrayContent CreateFakeImage(string fileName = "test.png")
    {
        // minimal valid PNG header bytes
        var bytes = new byte[]
        {
            137, 80, 78, 71, 13, 10, 26, 10
        };

        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        return content;
    }

    public static ByteArrayContent CreateRealImage(string fileName = "test.png")
    {
        using var image = new Image<Rgba32>(10, 10);

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);

        var content = new ByteArrayContent(ms.ToArray());
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        return content;
    }
}