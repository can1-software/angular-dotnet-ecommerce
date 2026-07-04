using System.Text;

using ECommerce.API.Data;

using ECommerce.API.Services;

using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;



var builder = WebApplication.CreateBuilder(args);



// ---------------------------------------------------------------------------

// 1) SERVİSLERİN KAYDI

// ---------------------------------------------------------------------------



builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();



builder.Services.AddDbContext<AppDbContext>(options =>

    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();



// CORS: Angular farklı portta çalışabilir (4200, 56415 vb.)

builder.Services.AddCors(options =>

{

    options.AddPolicy("AngularPolicy", policy =>

    {

        policy.SetIsOriginAllowed(origin =>

        {

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))

                return false;

            return uri.Host is "localhost" or "127.0.0.1";

        })

        .AllowAnyHeader()

        .AllowAnyMethod();

    });

});



var jwtKey = builder.Configuration["Jwt:Key"]!;

var jwtIssuer = builder.Configuration["Jwt:Issuer"];

var jwtAudience = builder.Configuration["Jwt:Audience"];



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



// Rol tabanlı yetkilendirme (Admin / Customer).

builder.Services.AddAuthorization();



var app = builder.Build();



// ---------------------------------------------------------------------------

// 2) VERİTABANI SEED (varsayılan admin)

// ---------------------------------------------------------------------------

using (var scope = app.Services.CreateScope())

{

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    await DbSeeder.SeedAsync(db);

}



// ---------------------------------------------------------------------------

// 3) HTTP PIPELINE

// ---------------------------------------------------------------------------



if (app.Environment.IsDevelopment())

{

    app.UseSwagger();

    app.UseSwaggerUI();

}



app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AngularPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();



app.Run();


