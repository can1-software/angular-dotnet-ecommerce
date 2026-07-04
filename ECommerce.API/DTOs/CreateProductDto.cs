using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    [MaxLength(500)]
    public string? MetaKeywords { get; set; }

    [Range(0.01, 9999999, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stok negatif olamaz.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Kategori seçilmelidir.")]
    public int CategoryId { get; set; }
}
