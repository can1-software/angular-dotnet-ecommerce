using ECommerce.API.DTOs;
using ECommerce.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

// [ApiController] → model doğrulamayı otomatik yapar, hata cevaplarını düzenler.
// Route → bu controller'daki adresler "/api/auth/..." ile başlar.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // Servisi Dependency Injection ile alıyoruz.
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        // İş kuralı hatası varsa (örn. e-posta zaten kayıtlı) 400 döneriz.
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        // Başarılıysa 200 ve token içeren cevabı döneriz.
        return Ok(result.Data);
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        // Kimlik doğrulama başarısızsa 401 (Unauthorized) döneriz.
        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(result.Data);
    }
}
