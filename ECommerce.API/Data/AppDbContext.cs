using ECommerce.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Data;

// DbContext, uygulama ile veritabanı arasındaki köprüdür.
// EF Core bu sınıf üzerinden sorguları SQL'e çevirir.
public class AppDbContext : DbContext
{
    // Bağlantı ayarları dışarıdan (Program.cs) verilir.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Her DbSet, veritabanında bir tabloya karşılık gelir.
    // "Users" adında bir tablo oluşacak.
    public DbSet<User> Users => Set<User>();

    // Tablo detaylarını burada yapılandırıyoruz.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aynı e-posta ile iki kez kayıt olunmasını engelle (benzersiz index).
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
