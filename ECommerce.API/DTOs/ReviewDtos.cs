using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsOwnReview { get; set; }
}

public class ProductReviewSummaryDto
{
    public double AverageRating { get; set; }
    public int TotalCount { get; set; }
    public List<ReviewDto> Items { get; set; } = new();
}

public class CreateReviewDto
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    public int ProductId { get; set; }

    [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Yorum metni zorunludur.")]
    [MinLength(5, ErrorMessage = "Yorum en az 5 karakter olmalıdır.")]
    [MaxLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
    public string Comment { get; set; } = string.Empty;
}
