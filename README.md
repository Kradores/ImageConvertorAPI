# Image Converter API

A lightweight ASP.NET Core Web API for converting and resizing images.  
The API accepts multiple uploaded images, converts them to WebP format, optionally resizes them into multiple dimensions, and returns the results as a ZIP archive.

The project uses:

- **SixLabors.ImageSharp** for image processing (resizing, encoding, format conversion)
- **Swagger (Swashbuckle)** for interactive API documentation and testing

---

## Features

- Upload up to 100 images per request
- Convert images to WebP
- Optional resizing to multiple dimensions
- Lossy or lossless compression
- Returns all processed images in a ZIP archive
- Interactive Swagger UI for testing

---

## Technologies Used

- .NET 10
- ASP.NET Core Web API
- SixLabors.ImageSharp
- SixLabors.ImageSharp.Formats.Webp
- Swagger / Swashbuckle

---

## Running Locally

### 1. Clone the repository

```bash
git clone https://github.com/your-username/your-repository.git
cd your-repository
```

### 2. Restore dependencies
```bash
dotnet restore
```

### 3. Build the project
```bash
dotnet build
```

### 4. Run the API
```bash
dotnet run
```

### 5. Open Swagger UI
After running, open your browser and navigate to:
```bash
https://localhost:{PORT}/swagger
```
or
```bash
http://localhost:{PORT}/swagger
```

You can now upload images and test the conversion endpoint directly from Swagger.

## Notes
- Maximum 100 files per request
- Supported input formats depend on ImageSharp capabilities (JPEG, PNG, GIF, BMP, WebP, etc.)
- Output format: WebP
- Large images are processed sequentially to minimize memory usage
