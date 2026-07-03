using ECommerce.API.DTOs;

namespace ECommerce.API.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, CategoryDto? Data)> CreateAsync(CreateCategoryDto dto);
    Task<(bool Success, string Message, CategoryDto? Data)> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
