using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
