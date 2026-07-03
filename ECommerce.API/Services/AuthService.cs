using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.API.Services;

// IAuthService arayüzünün gerçek uygulaması.
// Kayıt, giriş ve JWT token üretme işlerini burada yapıyoruz.
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;      // Veritabanı erişimi
    private readonly IConfiguration _config;     // appsettings.json ayarlarına erişim

    // Bu bağımlılıklar (dependency) dışarıdan otomatik verilir (Dependency Injection).
    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterAsync(RegisterDto dto)
    {
        // 1) Bu e-posta ile daha önce kayıt olunmuş mu kontrol et.
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
        {
            return (false, "Bu e-posta zaten kayıtlı.", null);
        }

        // 2) Yeni kullanıcı nesnesi oluştur. Şifreyi BCrypt ile hash'liyoruz.
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        // 3) Veritabanına ekle ve kaydet.
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 4) Kayıt başarılı; kullanıcıya bir token üret ve geri dön.
        var response = new AuthResponseDto
        {
            FullName = user.FullName,
            Email = user.Email,
            Token = GenerateJwtToken(user)
        };

        return (true, "Kayıt başarılı.", response);
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto dto)
    {
        // 1) E-postaya göre kullanıcıyı bul.
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        // 2) Kullanıcı yoksa VEYA şifre yanlışsa aynı genel mesajı döneriz.
        //    (Güvenlik için "e-posta mı şifre mi yanlış" bilgisini vermeyiz.)
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return (false, "E-posta veya şifre hatalı.", null);
        }

        // 3) Doğrulama başarılı; token üret ve geri dön.
        var response = new AuthResponseDto
        {
            FullName = user.FullName,
            Email = user.Email,
            Token = GenerateJwtToken(user)
        };

        return (true, "Giriş başarılı.", response);
    }

    // Kullanıcı için JWT (JSON Web Token) üretir.
    // Token, kullanıcının kimliğini taşıyan imzalı bir metindir.
    private string GenerateJwtToken(User user)
    {
        // appsettings.json içindeki gizli anahtar (imzalama için kullanılır).
        var secretKey = _config["Jwt:Key"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Token'ın içine gömeceğimiz bilgiler (claims).
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("fullName", user.FullName)
        };

        // Token'ı oluştur: kim yayınladı, kim için, ne zaman geçersiz olacak vb.
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // 2 saat geçerli
            signingCredentials: credentials
        );

        // Token nesnesini string'e çevir.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
