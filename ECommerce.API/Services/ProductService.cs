using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using ECommerce.API.Helpers;
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

    public async Task<PagedResultDto<ProductDto>> GetPagedAsync(string? search, int? categoryId, string? categorySlug, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 50);

        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(categorySlug))
            query = query.Where(p => p.Category.Slug == categorySlug.Trim());
        else if (categoryId is > 0)
            query = query.Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Slug.ToLower().Contains(term) ||
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
            Items = products.Select(p => ToDto(p, includeGallery: false)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await LoadProductQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : ToDto(product, includeGallery: true);
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        var product = await LoadProductQuery()
            .FirstOrDefaultAsync(p => p.Slug == slug);

        return product is null ? null : ToDto(product, includeGallery: true);
    }

    public async Task<(bool Success, string Message, ProductDto? Data)> CreateAsync(
        CreateProductDto dto, IFormFile? image, IEnumerable<IFormFile>? additionalImages)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            return (false, "Seçilen kategori bulunamadı.", null);

        var slug = await SlugHelper.GenerateUniqueAsync(
            dto.Name,
            s => _context.Products.AnyAsync(p => p.Slug == s));

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Slug = slug,
            Description = dto.Description,
            MetaTitle = dto.MetaTitle?.Trim(),
            MetaDescription = dto.MetaDescription?.Trim(),
            MetaKeywords = dto.MetaKeywords?.Trim(),
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId
        };

        try
        {
            await AddImagesAsync(product, image, additionalImages);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message, null);
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        await _context.Entry(product).Collection(p => p.Images).LoadAsync();

        return (true, "Ürün oluşturuldu.", ToDto(product, includeGallery: true));
    }

    public async Task<(bool Success, string Message, ProductDto? Data)> UpdateAsync(
        int id, UpdateProductDto dto, IFormFile? image, IEnumerable<IFormFile>? additionalImages)
    {
        var product = await LoadProductQuery()
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
                var mainImage = product.Images.OrderBy(i => i.SortOrder).FirstOrDefault();
                if (mainImage is not null)
                {
                    _fileStorage.DeleteProductImageIfExists(mainImage.ImageUrl);
                    mainImage.ImageUrl = newImageUrl;
                }
                else
                {
                    product.Images.Add(new ProductImage { ImageUrl = newImageUrl, SortOrder = 0 });
                }
                product.ImageUrl = newImageUrl;
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message, null);
            }
        }

        if (additionalImages is not null)
        {
            try
            {
                var nextOrder = product.Images.Count == 0 ? 0 : product.Images.Max(i => i.SortOrder) + 1;
                foreach (var file in additionalImages)
                {
                    var url = await _fileStorage.SaveProductImageAsync(file);
                    product.Images.Add(new ProductImage { ImageUrl = url, SortOrder = nextOrder++ });
                    product.ImageUrl ??= url;
                }
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message, null);
            }
        }

        var nameChanged = product.Name != dto.Name.Trim();
        product.Name = dto.Name.Trim();
        product.Description = dto.Description;
        product.MetaTitle = dto.MetaTitle?.Trim();
        product.MetaDescription = dto.MetaDescription?.Trim();
        product.MetaKeywords = dto.MetaKeywords?.Trim();
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;

        if (nameChanged)
        {
            product.Slug = await SlugHelper.GenerateUniqueAsync(
                product.Name,
                s => _context.Products.AnyAsync(p => p.Slug == s && p.Id != id));
        }

        await _context.SaveChangesAsync();

        return (true, "Ürün güncellendi.", ToDto(product, includeGallery: true));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return (false, "Ürün bulunamadı.");

        foreach (var img in product.Images)
            _fileStorage.DeleteProductImageIfExists(img.ImageUrl);

        _fileStorage.DeleteProductImageIfExists(product.ImageUrl);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return (true, "Ürün silindi.");
    }

    private IQueryable<Product> LoadProductQuery() =>
        _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images);

    private async Task AddImagesAsync(Product product, IFormFile? mainImage, IEnumerable<IFormFile>? additionalImages)
    {
        var order = 0;
        if (mainImage is not null)
        {
            var url = await _fileStorage.SaveProductImageAsync(mainImage);
            product.Images.Add(new ProductImage { ImageUrl = url, SortOrder = order++ });
            product.ImageUrl = url;
        }

        if (additionalImages is null) return;

        foreach (var file in additionalImages)
        {
            var url = await _fileStorage.SaveProductImageAsync(file);
            product.Images.Add(new ProductImage { ImageUrl = url, SortOrder = order++ });
            product.ImageUrl ??= url;
        }
    }

    private static ProductDto ToDto(Product p, bool includeGallery) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        Description = p.Description,
        MetaTitle = p.MetaTitle,
        MetaDescription = p.MetaDescription,
        MetaKeywords = p.MetaKeywords,
        Price = p.Price,
        Stock = p.Stock,
        CategoryId = p.CategoryId,
        CategorySlug = p.Category?.Slug ?? string.Empty,
        CategoryName = p.Category?.Name ?? string.Empty,
        ImageUrl = p.ImageUrl,
        Images = includeGallery
            ? p.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                SortOrder = i.SortOrder
            }).ToList()
            : new List<ProductImageDto>(),
        CreatedAt = p.CreatedAt
    };
}
