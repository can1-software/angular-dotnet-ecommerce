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

        // Örnek kategoriler (geliştirme için 20 adet).
        await SeedCategoriesAsync(context);
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        var dummyCategories = new[]
        {
            ("Elektronik", "Televizyon, ses sistemleri ve elektronik cihazlar"),
            ("Bilgisayar & Tablet", "Dizüstü, masaüstü bilgisayarlar ve tabletler"),
            ("Telefon & Aksesuar", "Akıllı telefonlar, kılıflar ve şarj cihazları"),
            ("Ev & Yaşam", "Ev dekorasyonu ve yaşam ürünleri"),
            ("Mutfak Gereçleri", "Tencere, tava ve mutfak aletleri"),
            ("Mobilya", "Oturma grubu, yatak odası ve ofis mobilyaları"),
            ("Giyim & Moda", "Kadın, erkek ve çocuk giyim ürünleri"),
            ("Ayakkabı", "Spor, günlük ve klasik ayakkabı modelleri"),
            ("Spor & Outdoor", "Fitness, kamp ve outdoor ekipmanları"),
            ("Kozmetik & Kişisel Bakım", "Cilt bakımı, makyaj ve parfüm"),
            ("Bebek & Çocuk", "Bebek bakımı ve çocuk ürünleri"),
            ("Kitap & Kırtasiye", "Kitaplar, defterler ve ofis malzemeleri"),
            ("Otomotiv", "Araç bakım ve oto aksesuar ürünleri"),
            ("Yapı Market", "El aletleri, boya ve hırdavat malzemeleri"),
            ("Süpermarket", "Gıda, içecek ve temizlik ürünleri"),
            ("Pet Shop", "Evcil hayvan maması ve aksesuarları"),
            ("Oyuncak & Hobi", "Oyuncaklar, puzzle ve hobi malzemeleri"),
            ("Müzik & Enstrüman", "Gitar, klavye ve müzik aksesuarları"),
            ("Saat & Aksesuar", "Kol saatleri, takı ve aksesuarlar"),
            ("Sağlık & Medikal", "Medikal cihazlar ve sağlık ürünleri"),
        };

        var existingNames = await context.Categories
            .Select(c => c.Name)
            .ToListAsync();

        var toAdd = dummyCategories
            .Where(c => !existingNames.Contains(c.Item1))
            .Select(c => new Category
            {
                Name = c.Item1,
                Description = c.Item2,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (toAdd.Count == 0) return;

        context.Categories.AddRange(toAdd);
        await context.SaveChangesAsync();
    }
}
