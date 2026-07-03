using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category is null ? null : ToDto(category);
    }

    public async Task<(bool Success, string Message, CategoryDto? Data)> CreateAsync(CreateCategoryDto dto)
    {
        var exists = await _context.Categories.AnyAsync(c => c.Name == dto.Name);
        if (exists)
            return (false, "Bu isimde bir kategori zaten var.", null);

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim()
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return (true, "Kategori oluşturuldu.", ToDto(category));
    }

    public async Task<(bool Success, string Message, CategoryDto? Data)> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
            return (false, "Kategori bulunamadı.", null);

        var nameTaken = await _context.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id);
        if (nameTaken)
            return (false, "Bu isimde başka bir kategori var.", null);

        category.Name = dto.Name.Trim();
        category.Description = dto.Description?.Trim();
        await _context.SaveChangesAsync();

        return (true, "Kategori güncellendi.", ToDto(category));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
            return (false, "Kategori bulunamadı.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return (true, "Kategori silindi.");
    }

    private static CategoryDto ToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        CreatedAt = c.CreatedAt
    };
}
