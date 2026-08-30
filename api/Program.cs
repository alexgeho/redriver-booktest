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
    // nobody can edit or delete them (enforced in QuotesController via IsSeed).
    //
    // The five featured quotes are reconciled on every startup: we remove any
    // existing seed quotes and re-insert the desired set. This is idempotent and
    // survives a stale/persisted database volume (a plain "seed only if empty"
    // guard would leave an old DB with the wrong — or zero — featured quotes).
    // User-added quotes (IsSeed = false) are never touched.
    var featuredQuotes = new[]
    {
        new Quote { IsSeed = true, Text = "Talk is cheap. Show me the code.", Author = "Linus Torvalds" },
        new Quote { IsSeed = true, Text = "Programs must be written for people to read, and only incidentally for machines to execute.", Author = "Harold Abelson" },
        new Quote { IsSeed = true, Text = "First, solve the problem. Then, write the code.", Author = "John Johnson" },
        new Quote { IsSeed = true, Text = "Any fool can write code that a computer can understand. Good programmers write code that humans can understand.", Author = "Martin Fowler" },
        new Quote { IsSeed = true, Text = "Simplicity is the soul of efficiency.", Author = "Austin Freeman" }
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
