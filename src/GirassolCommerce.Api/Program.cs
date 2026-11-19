using System.Text;
using GirassolCommerce.Api.Data;
using GirassolCommerce.Api.Models;
using GirassolCommerce.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>() ?? new MongoDbSettings();
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
var storageMode = builder.Configuration["Storage:Mode"] ?? "Auto";
if (storageMode is not ("Auto" or "MongoDB" or "InMemory"))
{
    throw new InvalidOperationException("Storage:Mode deve ser Auto, MongoDB ou InMemory.");
}

var mongoConnection = Environment.GetEnvironmentVariable("MONGODB_URI") ?? mongoSettings.ConnectionString;
if (string.IsNullOrWhiteSpace(mongoConnection))
{
    mongoConnection = "mongodb://localhost:27017";
}

mongoSettings.ConnectionString = mongoConnection;

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? jwtSettings.SecretKey;
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    jwtSecret = "dev-secret-key-muda-antes-de-produzir";
}

jwtSettings.SecretKey = jwtSecret;

builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = mongoConnection;
    options.DatabaseName = mongoSettings.DatabaseName;
});

builder.Services.Configure<JwtSettings>(options =>
{
    options.SecretKey = jwtSecret;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.ExpirationMinutes = jwtSettings.ExpirationMinutes;
});

if (storageMode == "InMemory")
{
    builder.Services.AddSingleton<ICommerceStore, InMemoryCommerceStore>();
}
else
{
    var clientSettings = MongoClientSettings.FromConnectionString(mongoConnection);
    clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(2);
    clientSettings.ConnectTimeout = TimeSpan.FromSeconds(2);
    var mongoClient = new MongoClient(clientSettings);

    try
    {
        await mongoClient.GetDatabase(mongoSettings.DatabaseName)
            .RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
        builder.Services.AddSingleton(mongoClient);
        builder.Services.AddSingleton<IMongoDatabase>(mongoClient.GetDatabase(mongoSettings.DatabaseName));
        builder.Services.AddSingleton<MongoDbContext>();
        builder.Services.AddSingleton<ICommerceStore, MongoCommerceStore>();
    }
    catch (Exception ex) when (storageMode == "Auto" && (ex is MongoException or TimeoutException))
    {
        Console.WriteLine($"MongoDB indisponível ({ex.Message}); iniciando com armazenamento em memória (dados não persistem após encerrar a API).");
        builder.Services.AddSingleton<ICommerceStore, InMemoryCommerceStore>();
    }
}

builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<PedidoBusinessService>();
builder.Services.AddSingleton(jwtSettings);

var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");
    app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html"));
}

app.UseCors("FrontendPolicy");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var store = app.Services.GetRequiredService<ICommerceStore>();
var adminEmail = "admin@girassol.com";
var adminExists = await store.BuscarUsuarioPorEmailAsync(adminEmail) is not null;
if (!adminExists)
{
    var admin = new Usuario
    {
        Nome = "Administrador Girassol",
        Email = adminEmail,
        SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        Role = "Admin",
        CriadoEm = DateTime.UtcNow
    };

    await store.InserirUsuarioAsync(admin);
}

var produtosExistentes = await store.ListarProdutosAsync();
if (produtosExistentes.Count == 0)
{
    var produtosSeed = new[]
    {
        new Produto
        {
            Nome = "Caneca Girassol",
            Slug = "caneca-girassol",
            Descricao = "Caneca em cerâmica com design floral.",
            Categoria = "Casa",
            Preco = 59.90m,
            Estoque = 12,
            Ativo = true,
            ImagemUrl = "https://images.unsplash.com/photo-1514228742587-6b1558fcca3d?auto=format&fit=crop&w=900&q=80",
            Tags = new[] { "girassol", "casa", "ceramica" },
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        },
        new Produto
        {
            Nome = "Camiseta Verde",
            Slug = "camiseta-verde",
            Descricao = "Camiseta básica de algodão com visual natural.",
            Categoria = "Moda",
            Preco = 89.00m,
            Estoque = 7,
            Ativo = true,
            ImagemUrl = "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?auto=format&fit=crop&w=900&q=80",
            Tags = new[] { "moda", "verde", "algodao" },
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        },
        new Produto
        {
            Nome = "Kit de Jardinagem",
            Slug = "kit-jardinagem",
            Descricao = "Conjunto para cuidados de plantas e jardinagem.",
            Categoria = "Jardinagem",
            Preco = 129.90m,
            Estoque = 5,
            Ativo = true,
            ImagemUrl = "https://images.unsplash.com/photo-1466692476868-aef1dfb1e735?auto=format&fit=crop&w=900&q=80",
            Tags = new[] { "jardinagem", "kit", "plantas" },
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        }
    };

    foreach (var produto in produtosSeed)
    {
        await store.InserirProdutoAsync(produto);
    }
}

app.MapFallbackToFile("index.html");
app.Run();
