using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CatalogoGalacticoApi.Data;
using CatalogoGalacticoApi.Models;
using CatalogoGalacticoApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Base de Datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// 2. Controladores y Especificación OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 3. Registro de Servicios (Seguridad y Negocio)
builder.Services.AddSingleton<ICifradoService, CifradoService>();
builder.Services.AddScoped<IPasswordHasher<Personaje>, PasswordHasher<Personaje>>();
builder.Services.AddScoped<IPersonajeService, PersonajeService>();
builder.Services.AddScoped<ICardPersonajeService, CardPersonajeService>();
builder.Services.AddScoped<IEventoService, EventoService>();

var app = builder.Build();

// 4. Activar Swagger UI apuntando al JSON de OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Catálogo Galáctico v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();