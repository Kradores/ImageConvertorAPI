using ImageConvertorAPI.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO.Compression;

namespace ImageConvertorAPI.Services;

public class ImageConversionService : IImageConversionService
{
    private static readonly string[] AllowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".webp", ".bmp"
    };

    public async Task<byte[]> ConvertAsync(
    ConvertRequest request,
    CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();

        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var sizes = ParseSizes(request.Sizes);

            foreach (var file in request.Files)
            {
                ValidateFile(file);

                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);

                using var image = Image.Load(file.OpenReadStream());

                foreach (var size in sizes)
                {
                    using var resized = image.Clone(ctx =>
                        ctx.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(size, size)
                        }));

                    using var temp = new MemoryStream();
                    var encoder = request.Lossless
                        ? new WebpEncoder
                        {
                            FileFormat = WebpFileFormatType.Lossless
                        }
                        : new WebpEncoder
                        {
                            Quality = request.Quality,
                            Method = (WebpEncodingMethod)request.CompressionLevel
                        };

                    resized.Save(temp, encoder);
                    temp.Position = 0;

                    var entry = zip.CreateEntry(
                        $"{fileNameWithoutExt}/{fileNameWithoutExt}-{size}.webp",
                        CompressionLevel.NoCompression);
                    using var entryStream = entry.Open();
                    await temp.CopyToAsync(entryStream, cancellationToken);
                }
            }
        }

        return ms.ToArray();
    }


    private static List<int> ParseSizes(string sizes)
    {
        return sizes
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.Parse(s.Trim()))
            .Where(s => s > 0)
            .Distinct()
            .OrderByDescending(s => s)
            .ToList();
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
            throw new BadHttpRequestException("Empty file.");

        if (file.Length > 50 * 1024 * 1024)
            throw new BadHttpRequestException("File exceeds 50MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new BadHttpRequestException("Unsupported file format.");
    }
}
