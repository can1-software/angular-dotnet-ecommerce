using System.Text;
using ECommerce.API.Data;
using ECommerce.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1) SERVİSLERİN KAYDI (Dependency Injection container'a ekleniyor)
// ---------------------------------------------------------------------------

// Controller'ları etkinleştir.
builder.Services.AddControllers();

// Swagger: API'yi tarayıcıdan test edebileceğimiz arayüz.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Veritabanı bağlantısını (SQL Server) EF Core'a tanıt.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Kendi servisimizi kaydediyoruz. Bir istek geldiğinde IAuthService istenirse,
// arka planda AuthService örneği üretilir (Scoped = her HTTP isteği için bir tane).
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT ayarlarını okuyalım.
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// JWT tabanlı kimlik doğrulamayı yapılandır.
// Bu, ileride "sadece giriş yapmış kullanıcılar erişebilsin" dediğimiz
// endpoint'ler için gelen token'ları doğrular.
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

// ---------------------------------------------------------------------------
// 2) HTTP İSTEK BORU HATTI (middleware sırası önemlidir)
// ---------------------------------------------------------------------------

// Sadece geliştirme ortamında Swagger arayüzünü aç.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Önce "kimsin?" (Authentication), sonra "yetkin var mı?" (Authorization).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
