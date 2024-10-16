using Microsoft.EntityFrameworkCore;

namespace TP_Labo4.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PeliculaActores>()
            .HasKey(pa => new { pa.PeliculaId, pa.ActorId });

            modelBuilder.Entity<PeliculaActores>()
                .HasOne(pa => pa.Pelicula)
                .WithMany(p => p.PeliculaActores)
                .HasForeignKey(pa => pa.PeliculaId);

            modelBuilder.Entity<PeliculaActores>()
                .HasOne(pa => pa.Actor)
                .WithMany(a => a.PeliculaActores)
                .HasForeignKey(pa => pa.ActorId);
        }
        public DbSet<Actor> Actores { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<PeliculaActores> PeliculaActores { get; set; }        
    }
}
