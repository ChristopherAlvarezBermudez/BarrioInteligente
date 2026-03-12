using Microsoft.EntityFrameworkCore;
using BarrioInteligenteWeb.Models;

namespace BarrioInteligenteWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Reporte> Reportes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<ComentarioLike> ComentariosLikes { get; set; }
        public DbSet<HistorialEstado> HistorialEstados { get; set; }
        public DbSet<Validacion> Validaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reporte>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Validacion>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones para el sistema de Likes de Comentarios
            modelBuilder.Entity<ComentarioLike>()
                .HasIndex(cl => new { cl.ComentarioId, cl.UsuarioId })
                .IsUnique();

            modelBuilder.Entity<ComentarioLike>()
                .HasOne(cl => cl.Comentario)
                .WithMany(c => c.Likes)
                .HasForeignKey(cl => cl.ComentarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComentarioLike>()
                .HasOne(cl => cl.Usuario)
                .WithMany()
                .HasForeignKey(cl => cl.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
