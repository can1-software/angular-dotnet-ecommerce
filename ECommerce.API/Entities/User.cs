namespace ECommerce.API.Entities;

// Bu sınıf, veritabanındaki "Users" tablosunun bir satırını temsil eder.
// Her özellik (property) bir sütuna karşılık gelir.
public class User
{
    // Birincil anahtar (primary key). EF Core "Id" ismini otomatik olarak PK kabul eder.
    public int Id { get; set; }

    // Kullanıcının adı
    public string FullName { get; set; } = string.Empty;

    // Giriş için kullanılacak e-posta (benzersiz olacak)
    public string Email { get; set; } = string.Empty;

    // DİKKAT: Şifreyi düz metin olarak DEĞİL, hash'lenmiş halini saklarız.
    // Böylece veritabanı ele geçse bile gerçek şifreler görünmez.
    public string PasswordHash { get; set; } = string.Empty;

    // Kaydın oluşturulma tarihi
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
