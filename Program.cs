using System.Text;
using System.Threading.RateLimiting;
using CatalogoGalacticoApi.Data;
using CatalogoGalacticoApi.Middleware;
using CatalogoGalacticoApi.Models;
using CatalogoGalacticoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((contexto, configuracion) => configuracion
    .ReadFrom.Configuration(contexto.Configuration)
    .Enrich.FromLogContext());

// 1. Base de Datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// 2. Controladores y Especificación OpenAPI / Swagger con soporte Bearer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Ingrese el token JWT. No incluya el prefijo Bearer."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 3. Registro de Servicios (Seguridad y Negocio)
builder.Services.AddSingleton<ICifradoService, CifradoService>();
builder.Services.AddScoped<IPasswordHasher<Personaje>, PasswordHasher<Personaje>>();
builder.Services.AddScoped<IPersonajeService, PersonajeService>();
builder.Services.AddScoped<ICardPersonajeService, CardPersonajeService>();
builder.Services.AddScoped<IEventoService, EventoService>();
builder.Services.AddScoped<IPlanetaService, PlanetaService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 4. CORS por configuración
var origenesPermitidos = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaCors", policy =>
    {
        policy.WithOrigins(origenesPermitidos)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 5. Rate Limiting (Global y Login)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
    {
        var cliente = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

        return RateLimitPartition.GetFixedWindowLimiter(cliente, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    options.AddPolicy("login", contexto =>
    {
        var cliente = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

        return RateLimitPartition.GetFixedWindowLimiter(cliente, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

// 6. Autenticación JWT y Autorización
var jwtClave = builder.Configuration["Jwt:Clave"]
    ?? throw new InvalidOperationException("Falta el secreto Jwt:Clave");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Emisor"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtClave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "role",
            NameClaimType = "name"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorGlobalExcepciones>();

var app = builder.Build();

app.UseMiddleware<ContextoPeticionMiddleware>();
app.UseExceptionHandler();

// Seed inicial de Administrador leyendo credenciales desde configuración (User Secrets / env)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var adminEmail = config["SeedAdmin:Email"]?.Trim().ToLowerInvariant();
    var adminPassword = config["SeedAdmin:Password"];

    if (!string.IsNullOrWhiteSpace(adminEmail) &&
        !string.IsNullOrWhiteSpace(adminPassword) &&
        !await db.Personajes.AnyAsync(p => p.Rol == "Administrador"))
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Personaje>>();
        var cifrado = scope.ServiceProvider.GetRequiredService<ICifradoService>();
        var admin = new Personaje
        {
            Nombre = "Administrador Galáctico",
            Email = adminEmail,
            Rol = "Administrador",
            Especie = "Humano",
            Faccion = Faccion.Neutral,
            Afiliacion = "Administración",
            Estado = Estado.Vivo,
            FuerzaSensitivo = false,
            CoordenadasOrigen = cifrado.Cifrar("Coruscant 0,0,0")
        };
        admin.PasswordHash = hasher.HashPassword(admin, adminPassword);
        db.Personajes.Add(admin);
        await db.SaveChangesAsync();
    }
}

// 7. Activar Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Catálogo Galáctico v1");
        options.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();
app.UseCors("PoliticaCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
