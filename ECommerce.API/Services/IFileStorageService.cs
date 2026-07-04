namespace ECommerce.API.Services;

public interface IFileStorageService
{
    Task<string?> SaveProductImageAsync(IFormFile file);
    void DeleteProductImageIfExists(string? imageUrl);
    bool IsSeedImage(string? imageUrl);
}
