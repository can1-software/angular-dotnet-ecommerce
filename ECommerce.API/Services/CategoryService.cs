using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using ECommerce.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<CategoryDto>> GetPagedAsync(string? search, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 50);

        var query = _context.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Slug.ToLower().Contains(term) ||
                (c.Description != null && c.Description.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var categories = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResultDto<CategoryDto>
        {
            Items = categories.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category is null ? null : ToDto(category);
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug);

        return category is null ? null : ToDto(category);
    }

    public async Task<(bool Success, string Message, CategoryDto? Data)> CreateAsync(CreateCategoryDto dto)
    {
        var exists = await _context.Categories.AnyAsync(c => c.Name == dto.Name);
        if (exists)
            return (false, "Bu isimde bir kategori zaten var.", null);

        var slug = await SlugHelper.GenerateUniqueAsync(
            dto.Name,
            s => _context.Categories.AnyAsync(c => c.Slug == s));

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Slug = slug,
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

        var nameChanged = category.Name != dto.Name.Trim();
        category.Name = dto.Name.Trim();
        category.Description = dto.Description?.Trim();

        if (nameChanged)
        {
            category.Slug = await SlugHelper.GenerateUniqueAsync(
                category.Name,
                s => _context.Categories.AnyAsync(c => c.Slug == s && c.Id != id));
        }

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
        Slug = c.Slug,
        Description = c.Description,
        CreatedAt = c.CreatedAt
    };
}
