using FluentAssertions;
using ImageConvertorAPI.Tests;
using System.IO.Compression;
using System.Net;

public class ConvertEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ConvertEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region Validation Tests
    [Fact]
    public async Task Convert_Should_Return_Zip_With_Images()
    {
        // Arrange
        var content = new MultipartFormDataContent();

        var file = TestFileHelper.CreateRealImage();
        content.Add(file, "Files", "test.png");

        content.Add(new StringContent("100,200"), "Sizes");
        content.Add(new StringContent("75"), "Quality");
        content.Add(new StringContent("false"), "Lossless");
        content.Add(new StringContent("4"), "CompressionLevel");

        // Act
        var response = await _client.PostAsync("/api/convert", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/zip");

        var bytes = await response.Content.ReadAsByteArrayAsync();

        using var ms = new MemoryStream(bytes);
        using var zip = new ZipArchive(ms);

        zip.Entries.Should().HaveCount(2);

        zip.Entries.Should().Contain(e => e.FullName.Contains("100.webp"));
        zip.Entries.Should().Contain(e => e.FullName.Contains("200.webp"));
    }

    [Fact]
    public async Task Convert_Should_Return_400_When_No_Files()
    {
        var content = new MultipartFormDataContent();

        var response = await _client.PostAsync("/api/convert", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Convert_Should_Return_400_When_Too_Many_Files()
    {
        var content = new MultipartFormDataContent();

        for (int i = 0; i < 101; i++)
        {
            var file = TestFileHelper.CreateRealImage();
            content.Add(file, "Files", $"test{i}.png");
        }

        content.Add(new StringContent("100"), "Sizes");

        var response = await _client.PostAsync("/api/convert", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Convert_Should_Return_400_For_Unsupported_Format()
    {
        var content = new MultipartFormDataContent();

        var file = TestFileHelper.CreateRealImage();
        content.Add(file, "Files", "test.txt"); // extension matters

        content.Add(new StringContent("100"), "Sizes");

        var response = await _client.PostAsync("/api/convert", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Convert_Should_Return_400_For_Empty_File()
    {
        var content = new MultipartFormDataContent();

        var file = new ByteArrayContent(Array.Empty<byte>());
        content.Add(file, "Files", "test.png");

        content.Add(new StringContent("100"), "Sizes");

        var response = await _client.PostAsync("/api/convert", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Convert_Should_Return_400_For_Invalid_Image_Content()
    {
        var content = new MultipartFormDataContent();

        var file = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        content.Add(file, "Files", "test.png");

        content.Add(new StringContent("100"), "Sizes");

        var response = await _client.PostAsync("/api/convert", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Functional Tests

    [Fact]
    public async Task Convert_Should_Create_One_File_Per_Size()
    {
        var content = new MultipartFormDataContent();

        var file = TestFileHelper.CreateRealImage();
        content.Add(file, "Files", "test.png");

        content.Add(new StringContent("100,200,300"), "Sizes");

        var response = await _client.PostAsync("/api/convert", content);

        var zipBytes = await response.Content.ReadAsByteArrayAsync();

        using var zip = new ZipArchive(new MemoryStream(zipBytes));

        zip.Entries.Should().HaveCount(3);
    }

    [Fact]
    public async Task Convert_Should_Create_All_Combinations_Of_Files_And_Sizes()
    {
        var content = new MultipartFormDataContent();

        content.Add(TestFileHelper.CreateRealImage(), "Files", "a.png");
        content.Add(TestFileHelper.CreateRealImage(), "Files", "b.png");

        content.Add(new StringContent("100,200"), "Sizes");

        var response = await _client.PostAsync("/api/convert", content);

        var zipBytes = await response.Content.ReadAsByteArrayAsync();

        using var zip = new ZipArchive(new MemoryStream(zipBytes));

        zip.Entries.Should().HaveCount(4);
    }

    [Fact]
    public async Task Convert_Should_Generate_Correct_File_Names()
    {
        var content = new MultipartFormDataContent();

        content.Add(TestFileHelper.CreateRealImage(), "Files", "image.png");
        content.Add(new StringContent("100"), "Sizes");

        var response = await _client.PostAsync("/api/convert", content);

        var zipBytes = await response.Content.ReadAsByteArrayAsync();

        using var zip = new ZipArchive(new MemoryStream(zipBytes));

        zip.Entries.Should().Contain(e =>
            e.FullName == "image/image-100.webp");
    }

    [Fact]
    public async Task Convert_Should_Create_Non_Empty_Files()
    {
        var content = new MultipartFormDataContent();

        content.Add(TestFileHelper.CreateRealImage(), "Files", "image.png");
        content.Add(new StringContent("100"), "Sizes");

        var response = await _client.PostAsync("/api/convert", content);

        var zipBytes = await response.Content.ReadAsByteArrayAsync();

        using var zip = new ZipArchive(new MemoryStream(zipBytes));

        zip.Entries.All(e => e.Length > 0).Should().BeTrue();
    }

    #endregion

    #region Input Parsing Tests
    [Fact]
    public async Task Convert_Should_Remove_Duplicate_Sizes()
    {
        var content = new MultipartFormDataContent();

        content.Add(TestFileHelper.CreateRealImage(), "Files", "image.png");
        content.Add(new StringContent("100,100,200"), "Sizes");

        var response = await _client.PostAsync("/api/convert", content);

        var zipBytes = await response.Content.ReadAsByteArrayAsync();

        using var zip = new ZipArchive(new MemoryStream(zipBytes));

        zip.Entries.Should().HaveCount(2);
    }

    [Fact]
    public async Task Convert_Should_Process_Sizes_In_Descending_Order()
    {
        var content = new MultipartFormDataContent();

        content.Add(TestFileHelper.CreateRealImage(), "Files", "image.png");
        content.Add(new StringContent("100,300,200"), "Sizes");

        var response = await _client.PostAsync("/api/convert", content);

        var zipBytes = await response.Content.ReadAsByteArrayAsync();

        using var zip = new ZipArchive(new MemoryStream(zipBytes));

        var names = zip.Entries.Select(e => e.FullName).ToList();

        names.Should().ContainInOrder(
            "image/image-300.webp",
            "image/image-200.webp",
            "image/image-100.webp"
        );
    }

    #endregion
}