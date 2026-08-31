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

    // Quotes are entirely user-generated: every user adds their own favourites,
    // all quotes are visible to everyone, and only the owner can edit or delete
    // theirs. Nothing to seed here.
    //
    // Legacy schema fix: older databases created by a previous version have a
    // NOT NULL `IsSeed` column that this model no longer maps. Because we use
    // EnsureCreated() (not migrations), that column lingers and breaks inserts
    // (NOT NULL constraint). Drop it if present; on a fresh DB this is a no-op.
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Quotes DROP COLUMN IsSeed;"); }
    catch { /* column already absent — nothing to do */ }

    // One-time cleanup: earlier versions seeded 5 hardcoded "featured" quotes
    // with no owner (UserId = null). Those are obsolete now — remove any
    // ownerless quotes so only real user-added quotes remain.
    var orphanQuotes = db.Quotes.Where(q => q.UserId == null).ToList();
    if (orphanQuotes.Count > 0)
    {
        db.Quotes.RemoveRange(orphanQuotes);
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
