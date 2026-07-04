using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public ProductService(AppDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<PagedResultDto<ProductDto>> GetPagedAsync(string? search, int? categoryId, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 50);

        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (categoryId is > 0)
            query = query.Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                p.Category.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResultDto<ProductDto>
        {
            Items = products.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : ToDto(product);
    }

    public async Task<(bool Success, string Message, ProductDto? Data)> CreateAsync(CreateProductDto dto, IFormFile? image)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            return (false, "Seçilen kategori bulunamadı.", null);

        string? imageUrl = null;
        try
        {
            if (image is not null)
                imageUrl = await _fileStorage.SaveProductImageAsync(image);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message, null);
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId,
            ImageUrl = imageUrl
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        return (true, "Ürün oluşturuldu.", ToDto(product));
    }

    public async Task<(bool Success, string Message, ProductDto? Data)> UpdateAsync(int id, UpdateProductDto dto, IFormFile? image)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return (false, "Ürün bulunamadı.", null);

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            return (false, "Seçilen kategori bulunamadı.", null);

        if (image is not null)
        {
            try
            {
                var newImageUrl = await _fileStorage.SaveProductImageAsync(image);
                _fileStorage.DeleteProductImageIfExists(product.ImageUrl);
                product.ImageUrl = newImageUrl;
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message, null);
            }
        }

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim();
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();

        return (true, "Ürün güncellendi.", ToDto(product));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
            return (false, "Ürün bulunamadı.");

        _fileStorage.DeleteProductImageIfExists(product.ImageUrl);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return (true, "Ürün silindi.");
    }

    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Stock = p.Stock,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        ImageUrl = p.ImageUrl,
        CreatedAt = p.CreatedAt
    };
}
