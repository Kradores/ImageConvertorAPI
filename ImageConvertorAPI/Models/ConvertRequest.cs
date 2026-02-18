namespace ImageConvertorAPI.Models;

public class ConvertRequest
{
    public List<IFormFile> Files { get; set; } = [];
    public int Quality { get; set; }
    public int CompressionLevel { get; set; }
    public string Sizes { get; set; } = string.Empty;
    public bool Lossless { get; set; }
}
