using ImageConvertorAPI.Models;

namespace ImageConvertorAPI.Services;

public interface IImageConversionService
{
    Task<byte[]> ConvertAsync(
        ConvertRequest request,
        CancellationToken cancellationToken);
}
