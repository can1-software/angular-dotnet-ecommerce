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

        // Örnek ürünler (geliştirme için 20 adet, resimli).
        await SeedProductsAsync(context);
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

    private static async Task SeedProductsAsync(AppDbContext context)
    {
        var dummyProducts = new (string Name, string Category, string Description, decimal Price, int Stock, string ImageFile)[]
        {
            ("Samsung Galaxy S24", "Telefon & Aksesuar", "128 GB, AMOLED ekran, 50 MP kamera", 42999m, 35, "product-01.jpg"),
            ("iPhone 15 Pro", "Telefon & Aksesuar", "Titanium kasa, A17 Pro çip, 256 GB", 64999m, 20, "product-02.jpg"),
            ("MacBook Air M3", "Bilgisayar & Tablet", "13 inç, 16 GB RAM, 512 GB SSD", 54999m, 15, "product-03.jpg"),
            ("Lenovo IdeaPad 5", "Bilgisayar & Tablet", "15.6 inç, Ryzen 7, 16 GB RAM", 28999m, 28, "product-04.jpg"),
            ("Sony Bravia 55\"", "Elektronik", "4K UHD Smart TV, Dolby Vision", 37999m, 12, "product-05.jpg"),
            ("Philips Airfryer XXL", "Mutfak Gereçleri", "7.3 L kapasite, dijital ekran", 8999m, 45, "product-06.jpg"),
            ("IKEA Koltuk Takımı", "Mobilya", "3+2+1 oturma grubu, gri kumaş", 24999m, 8, "product-07.jpg"),
            ("Nike Air Max 90", "Ayakkabı", "Erkek spor ayakkabı, siyah-beyaz", 5499m, 60, "product-08.jpg"),
            ("Adidas Koşu Tişörtü", "Giyim & Moda", "Nefes alabilir kumaş, dry-fit", 1299m, 100, "product-09.jpg"),
            ("Decathlon Kamp Çadırı", "Spor & Outdoor", "4 kişilik, su geçirmez, hafif", 6999m, 22, "product-10.jpg"),
            ("L'Oréal Cilt Bakım Seti", "Kozmetik & Kişisel Bakım", "Nemlendirici + serum + temizleyici", 2499m, 55, "product-11.jpg"),
            ("Chicco Bebek Arabası", "Bebek & Çocuk", "Katlanabilir, 0-3 yaş uyumlu", 15999m, 14, "product-12.jpg"),
            ("Harry Potter Kutu Seti", "Kitap & Kırtasiye", "7 kitap, özel baskı koleksiyon", 1899m, 40, "product-13.jpg"),
            ("Bosch Akü Şarj Cihazı", "Otomotiv", "12V, akıllı şarj, LED göstergeli", 3499m, 30, "product-14.jpg"),
            ("Bosch Matkap Seti", "Yapı Market", "18V, 2 akü, 50 parça aksesuar", 7999m, 18, "product-15.jpg"),
            ("Nescafé Kahve Paketi", "Süpermarket", "200 gr x 6 adet, gold blend", 899m, 200, "product-16.jpg"),
            ("Royal Canin Kedi Maması", "Pet Shop", "10 kg, yetişkin kedi için", 3299m, 75, "product-17.jpg"),
            ("LEGO Star Wars Seti", "Oyuncak & Hobi", "1500+ parça, koleksiyonluk", 4999m, 25, "product-18.jpg"),
            ("Yamaha Akustik Gitar", "Müzik & Enstrüman", "Tam boy, maun gövde, parlak finish", 8999m, 10, "product-19.jpg"),
            ("Casio G-Shock Saat", "Saat & Aksesuar", "Dijital, su geçirmez, şok dayanımlı", 4499m, 42, "product-20.jpg"),
        };

        var existingNames = await context.Products
            .Select(p => p.Name)
            .ToListAsync();

        var categories = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

        var toAdd = new List<Product>();
        foreach (var (name, categoryName, description, price, stock, imageFile) in dummyProducts)
        {
            if (existingNames.Contains(name))
                continue;

            if (!categories.TryGetValue(categoryName, out var categoryId))
                continue;

            toAdd.Add(new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Stock = stock,
                CategoryId = categoryId,
                ImageUrl = $"/uploads/products/seed/{imageFile}",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (toAdd.Count == 0) return;

        context.Products.AddRange(toAdd);
        await context.SaveChangesAsync();
    }
}
