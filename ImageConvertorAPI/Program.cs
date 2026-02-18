using ImageConvertorAPI.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddResponseCompression();
builder.Services.AddScoped<IImageConversionService, ImageConversionService>();
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = (long)100 * 50 * 1024 * 1024; // 100 files * 50MB
});

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = (long)100 * 50 * 1024 * 1024;
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", config =>
    {
        config.PermitLimit = 20;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueLimit = 5;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "Open API v1");
    });
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseRateLimiter();

app.MapControllers().RequireRateLimiting("fixed");

app.Run();
