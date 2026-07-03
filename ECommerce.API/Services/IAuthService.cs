using ECommerce.API.DTOs;

namespace ECommerce.API.Services;

// Interface (arayüz), servisin "ne yaptığını" tanımlar, "nasıl yaptığını" değil.
// Controller bu arayüze bağlıdır; gerçek uygulama AuthService içindedir.
// Bu, kodu test edilebilir ve esnek yapar (OOP prensiplerinden bağımlılık soyutlama).
public interface IAuthService
{
    // Yeni kullanıcı kaydeder. Başarı durumu, mesaj ve cevap verisi döner.
    Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterAsync(RegisterDto dto);

    // Mevcut kullanıcıyı doğrular ve giriş yaptırır.
    Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto dto);
}
