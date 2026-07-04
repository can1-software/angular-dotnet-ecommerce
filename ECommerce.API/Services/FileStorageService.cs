namespace ECommerce.API.Services;

public class FileStorageService : IFileStorageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveProductImageAsync(IFormFile file)
    {
        if (file.Length == 0)
            return null;

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("Resim dosyası en fazla 5 MB olabilir.");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Sadece JPG, PNG veya WEBP dosyaları yüklenebilir.");

        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/products/{fileName}";
    }

    public void DeleteProductImageIfExists(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || IsSeedImage(imageUrl))
            return;

        var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public bool IsSeedImage(string? imageUrl) =>
        imageUrl?.Contains("/uploads/products/seed/", StringComparison.OrdinalIgnoreCase) == true;
}
