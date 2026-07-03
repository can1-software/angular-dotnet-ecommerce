using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

// DTO = Data Transfer Object.
// Kayıt olurken istemciden (Angular/Swagger) gelen veriyi taşır.
// Entity yerine DTO kullanırız; böylece dışarıya sadece gereken alanları açarız.
public class RegisterDto
{
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;
}
