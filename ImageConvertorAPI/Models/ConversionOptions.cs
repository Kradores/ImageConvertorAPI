namespace ImageConvertorAPI.Models;

public class ConversionOptions
{
    public int Quality { get; set; }
    public int CompressionLevel { get; set; }
    public bool Lossless { get; set; }
    public List<int> Sizes { get; set; } = [];
}
