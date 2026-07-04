using ECommerce.API.Entities;
using ECommerce.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Data;

// Uygulama ilk açıldığında varsayılan admin kullanıcısını oluşturur.
public static class DbSeeder
{
    public const string AdminEmail = "admin@novashop.com";
    public const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Users
            .Where(u => u.Role == "" || u.Role == null)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Role, UserRole.Customer));

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

        await SeedCategoriesAsync(context);
        await SeedProductsAsync(context);
        await EnsureSlugsAsync(context);
        await EnsureProductDetailsAsync(context);
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

        var existingSlugs = await context.Categories
            .Select(c => c.Slug)
            .ToListAsync();

        var toAdd = new List<Category>();
        foreach (var (name, description) in dummyCategories)
        {
            if (existingNames.Contains(name))
                continue;

            var slug = SlugHelper.Generate(name);
            var uniqueSlug = slug;
            var suffix = 2;
            while (existingSlugs.Contains(uniqueSlug))
            {
                uniqueSlug = $"{slug}-{suffix}";
                suffix++;
            }
            existingSlugs.Add(uniqueSlug);

            toAdd.Add(new Category
            {
                Name = name,
                Slug = uniqueSlug,
                Description = description,
                CreatedAt = DateTime.UtcNow
            });
        }

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

        var existingSlugs = await context.Products
            .Select(p => p.Slug)
            .ToListAsync();

        var categories = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

        var toAdd = new List<Product>();
        foreach (var (name, categoryName, description, price, stock, imageFile) in dummyProducts)
        {
            if (existingNames.Contains(name))
                continue;

            if (!categories.TryGetValue(categoryName, out var categoryId))
                continue;

            var slug = SlugHelper.Generate(name);
            var uniqueSlug = slug;
            var suffix = 2;
            while (existingSlugs.Contains(uniqueSlug))
            {
                uniqueSlug = $"{slug}-{suffix}";
                suffix++;
            }
            existingSlugs.Add(uniqueSlug);

            toAdd.Add(new Product
            {
                Name = name,
                Slug = uniqueSlug,
                Description = BuildHtmlDescription(name, description),
                MetaTitle = $"{name} | NovaShop",
                MetaDescription = $"{name} - {description}. NovaShop'ta uygun fiyat ve hızlı kargo.",
                MetaKeywords = $"{name}, novashop, online alışveriş",
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

    private static async Task EnsureSlugsAsync(AppDbContext context)
    {
        var categories = await context.Categories.ToListAsync();
        var categorySlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories.OrderBy(c => c.Id))
        {
            if (!NeedsSlug(category.Slug))
            {
                categorySlugs.Add(category.Slug);
                continue;
            }

            category.Slug = await SlugHelper.GenerateUniqueAsync(
                category.Name,
                s => Task.FromResult(categorySlugs.Contains(s)));
            categorySlugs.Add(category.Slug);
        }

        var products = await context.Products.ToListAsync();
        var productSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var product in products.OrderBy(p => p.Id))
        {
            if (!NeedsSlug(product.Slug))
            {
                productSlugs.Add(product.Slug);
                continue;
            }

            product.Slug = await SlugHelper.GenerateUniqueAsync(
                product.Name,
                s => Task.FromResult(productSlugs.Contains(s)));
            productSlugs.Add(product.Slug);
        }

        await context.SaveChangesAsync();
    }

    private static bool NeedsSlug(string? slug) =>
        string.IsNullOrWhiteSpace(slug) ||
        slug.StartsWith("product-", StringComparison.OrdinalIgnoreCase) ||
        slug.StartsWith("category-", StringComparison.OrdinalIgnoreCase);

    private static async Task EnsureProductDetailsAsync(AppDbContext context)
    {
        var products = await context.Products.Include(p => p.Images).ToListAsync();

        foreach (var product in products)
        {
            if (string.IsNullOrWhiteSpace(product.MetaTitle))
                product.MetaTitle = $"{product.Name} | NovaShop";

            if (string.IsNullOrWhiteSpace(product.MetaDescription))
                product.MetaDescription = $"{product.Name} - NovaShop'ta en uygun fiyat ve hızlı kargo.";

            if (string.IsNullOrWhiteSpace(product.MetaKeywords))
                product.MetaKeywords = $"{product.Name}, novashop, e-ticaret";

            if (string.IsNullOrWhiteSpace(product.Description) || !product.Description.Contains('<'))
            {
                var plain = product.Description ?? "Kaliteli ürün, hızlı teslimat.";
                product.Description = BuildHtmlDescription(product.Name, plain);
            }

            if (product.Images.Count > 0) continue;

            var sort = 0;
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = product.ImageUrl,
                    SortOrder = sort++
                });
            }

            var productNum = ((product.Id - 1) % 20) + 1;
            var alt1 = $"/uploads/products/seed/product-{((productNum % 20) + 1):D2}.jpg";
            var alt2 = $"/uploads/products/seed/product-{((productNum + 7) % 20) + 1:D2}.jpg";

            if (product.ImageUrl != alt1)
                product.Images.Add(new ProductImage { ImageUrl = alt1, SortOrder = sort++ });

            if (product.ImageUrl != alt2 && alt1 != alt2)
                product.Images.Add(new ProductImage { ImageUrl = alt2, SortOrder = sort++ });
        }

        await context.SaveChangesAsync();
    }

    private static string BuildHtmlDescription(string name, string summary) =>
        $"""
        <h2>{name}</h2>
        <p>{summary}</p>
        <h3>Ürün Avantajları</h3>
        <ul>
          <li>Orijinal ve garantili ürün</li>
          <li>Hızlı kargo seçeneği</li>
          <li>14 gün koşulsuz iade</li>
          <li>7/24 müşteri desteği</li>
        </ul>
        <h3>Teslimat &amp; İade</h3>
        <p>Siparişleriniz 1-3 iş günü içinde kargoya verilir. Ürünü teslim aldıktan sonra 14 gün içinde iade edebilirsiniz.</p>
        """;
}
