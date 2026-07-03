namespace ECommerce.API.DTOs;

// Başarılı kayıt/giriş sonrası istemciye döndürülen cevap.
// İçinde JWT token bulunur; istemci bu token'ı sonraki isteklerde kullanır.
public class AuthResponseDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
