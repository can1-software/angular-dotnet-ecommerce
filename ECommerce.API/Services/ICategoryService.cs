using ECommerce.API.DTOs;

namespace ECommerce.API.Services;

public interface ICategoryService
{
    Task<PagedResultDto<CategoryDto>> GetPagedAsync(string? search, int page, int pageSize);
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto?> GetBySlugAsync(string slug);
    Task<(bool Success, string Message, CategoryDto? Data)> CreateAsync(CreateCategoryDto dto);
    Task<(bool Success, string Message, CategoryDto? Data)> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
