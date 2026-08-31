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

    // Schema safety: a previous version dropped the IsSeed column. Because we use
    // EnsureCreated() (not migrations), re-add it if a persisted DB lacks it, so
    // the seed logic below doesn't hit "no such column". No-op on a fresh DB.
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Quotes ADD COLUMN IsSeed INTEGER NOT NULL DEFAULT 0;"); }
    catch { /* column already exists — nothing to do */ }

    // --- Seed 5 featured, read-only quotes ---
    // These belong to no user (UserId = null), are shown to everyone, and can
    // only be viewed — never edited or deleted (enforced in QuotesController via
    // IsSeed). They are reconciled on every startup (remove + re-insert) so the
    // set stays correct even on a persisted database. User-added quotes
    // (IsSeed = false) are never touched.
    var featuredQuotes = new[]
    {
        new Quote { IsSeed = true, Text = "You should name a variable using the same care with which you name a first-born child.", Author = "Robert C. Martin" },
        new Quote { IsSeed = true, Text = "In life there are important and unimportant things, mixing them up is a disaster.", Author = "" },
        new Quote { IsSeed = true, Text = "At its core, courage means: to risk the known for the unknown, the familiar for the unfamiliar, the comfortable for the uncomfortable. A person never knows if something will work out or not. It's a gamble — but only those who play know what life is.", Author = "" },
        new Quote { IsSeed = true, Text = "Through thorns to the stars!", Author = "" },
        new Quote { IsSeed = true, Text = "Don't wish it was easier, wish you were better. Don't wish for less problems, wish for more skills. Don't wish for less challenge, wish for more wisdom.", Author = "Jim Rohn" }
    };
    var existingSeeds = db.Quotes.Where(q => q.IsSeed).ToList();
    db.Quotes.RemoveRange(existingSeeds);
    db.Quotes.AddRange(featuredQuotes);
    db.SaveChanges();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
