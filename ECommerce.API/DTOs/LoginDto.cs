using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

// Giriş yaparken istemciden gelen veriyi taşır.
public class LoginDto
{
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    public string Password { get; set; } = string.Empty;
}
