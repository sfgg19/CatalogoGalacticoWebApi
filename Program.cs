using Microsoft.EntityFrameworkCore;
using CatalogoGalacticoApi.Data;
using CatalogoGalacticoApi.Services;
using CatalogoGalacticoApi.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena DefaultConnection.");

//dsf
// 1. Registrar utilidades de Seguridad
builder.Services.AddSingleton<ICifradoService, CifradoService>(); // Singleton porque no tiene estado y la clave es constante
builder.Services.AddScoped<IPasswordHasher<Personaje>, PasswordHasher<Personaje>>(); // El Hasher oficial de ASP.NET Core

// 2. Registrar Servicios de Negocio (Scoped para que vivan por cada petición HTTP)
builder.Services.AddScoped<IPersonajeService, PersonajeService>();
builder.Services.AddScoped<ICardPersonajeService, CardPersonajeService>();
builder.Services.AddScoped<IEventoService, EventoService>();
//dsf

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();