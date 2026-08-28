using System.Text;
using api.Data;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Honour the PORT env var (Render / most PaaS providers inject it).
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// --- Database (SQLite) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db"));

builder.Services.AddScoped<TokenService>();

// --- JWT authentication ---
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });
builder.Services.AddAuthorization();

// --- CORS (token is sent in the Authorization header, so any origin is safe here) ---
const string CorsPolicy = "spa";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BookQuotes API", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

// --- Create DB + seed a few books ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Books.Any())
    {
        db.Books.AddRange(
            new Book { Title = "The Pragmatic Programmer", Author = "Hunt & Thomas", PublishedDate = "1999-10-20" },
            new Book { Title = "Clean Code", Author = "Robert C. Martin", PublishedDate = "2008-08-01" },
            new Book { Title = "You Don't Know JS", Author = "Kyle Simpson", PublishedDate = "2015-12-27" }
        );
        db.SaveChanges();
    }

    // --- Seed 5 featured, read-only quotes ---
    // These belong to no user (UserId = null) and are shown to everyone, but
    // nobody can edit or delete them. Replace the text/author below with your
    // own five favourite quotes.
    if (!db.Quotes.Any(q => q.IsSeed))
    {
        db.Quotes.AddRange(
            new Quote { IsSeed = true, Text = "Favourite quote #1 — replace me.", Author = "Author 1" },
            new Quote { IsSeed = true, Text = "Favourite quote #2 — replace me.", Author = "Author 2" },
            new Quote { IsSeed = true, Text = "Favourite quote #3 — replace me.", Author = "Author 3" },
            new Quote { IsSeed = true, Text = "Favourite quote #4 — replace me.", Author = "Author 4" },
            new Quote { IsSeed = true, Text = "Favourite quote #5 — replace me.", Author = "Author 5" }
        );
        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
