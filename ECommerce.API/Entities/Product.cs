namespace ECommerce.API.Entities;

// Mağazadaki ürün. Her ürün bir kategoriye bağlıdır.
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
