using ECommerce.API.DTOs;

namespace ECommerce.API.Services;

public interface IProductService
{
    Task<PagedResultDto<ProductDto>> GetPagedAsync(string? search, int page, int pageSize);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, ProductDto? Data)> CreateAsync(CreateProductDto dto);
    Task<(bool Success, string Message, ProductDto? Data)> UpdateAsync(int id, UpdateProductDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
