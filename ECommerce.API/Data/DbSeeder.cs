using ECommerce.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Data;

// Uygulama ilk açıldığında varsayılan admin kullanıcısını oluşturur.
public static class DbSeeder
{
    // Varsayılan admin giriş bilgileri (geliştirme ortamı için).
    public const string AdminEmail = "admin@novashop.com";
    public const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(AppDbContext context)
    {
        // Eski kayıtlarda rol boşsa müşteri yap.
        await context.Users
            .Where(u => u.Role == "" || u.Role == null)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Role, UserRole.Customer));

        // Admin yoksa oluştur.
        var adminExists = await context.Users.AnyAsync(u => u.Email == AdminEmail);
        if (!adminExists)
        {
            context.Users.Add(new User
            {
                FullName = "Admin",
                Email = AdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword),
                Role = UserRole.Admin
            });
            await context.SaveChangesAsync();
        }
    }
}
