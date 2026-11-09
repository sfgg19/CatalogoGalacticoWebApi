using Microsoft.EntityFrameworkCore;
using CatalogoGalacticoApi.Models;

namespace CatalogoGalacticoApi.Data;

public class AppDbContext : DbContext
{
    // Constructor que recibe la conexión a MySQL inyectada desde Program.cs
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Colecciones que representan las tablas en MySQL
    public DbSet<Personaje> Personajes => Set<Personaje>();
    public DbSet<CardPersonaje> CardsPersonaje => Set<CardPersonaje>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Planeta> Planetas => Set<Planeta>();

    // Configuración estructural de la base de datos (Fluent API)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Reglas estrictas para Personaje
        modelBuilder.Entity<Personaje>(e =>
        {
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            e.HasIndex(p => p.Nombre).IsUnique(); // El nombre no se puede repetir

            // Requisitos de rúbrica: Los datos de seguridad son obligatorios
            e.Property(p => p.PasswordHash).IsRequired();
            e.Property(p => p.CoordenadasOrigen).IsRequired();

            // Configuración de la relación N:M (Personaje - Evento)
            e.HasMany(p => p.Eventos)
             .WithMany(ev => ev.Participantes)
             .UsingEntity("ParticipacionEvento"); // Nombra explícitamente la tabla intermedia invisible
        });

        // 2. Reglas para CardPersonaje y la Relación 1:1
        modelBuilder.Entity<CardPersonaje>(e =>
        {
            e.Property(c => c.HabilidadEspecial).HasMaxLength(150);
            e.Property(c => c.Arma).HasMaxLength(100);
            e.Property(c => c.ImagenUrl).HasMaxLength(255);

            // Relación 1:1 forzada
            e.HasOne(c => c.Personaje)
             .WithOne(p => p.Card)
             .HasForeignKey<CardPersonaje>(c => c.PersonajeId)
             .OnDelete(DeleteBehavior.Cascade); // Si se elimina un personaje, se elimina su carta automáticamente
        });

        // 3. Reglas para Planeta
        modelBuilder.Entity<Planeta>(e =>
        {
            e.Property(pl => pl.Nombre).IsRequired().HasMaxLength(100);
            e.HasIndex(pl => pl.Nombre).IsUnique();
            e.Property(pl => pl.Sistema).HasMaxLength(100);
            e.Property(pl => pl.Tipo).HasMaxLength(50);
            e.Property(pl => pl.Region).HasMaxLength(100);
        });

        // 4. Reglas para Evento y Relación 1:N con Planeta
        modelBuilder.Entity<Evento>(e =>
        {
            e.Property(ev => ev.Nombre).IsRequired().HasMaxLength(150);

            // Relación 1:N (Un Planeta tiene muchos Eventos)
            e.HasOne(ev => ev.Planeta)
             .WithMany(pl => pl.Eventos)
             .HasForeignKey(ev => ev.PlanetaId)
             .OnDelete(DeleteBehavior.Restrict); // Evita borrar planetas si tienen eventos vinculados
        });
    }
}
