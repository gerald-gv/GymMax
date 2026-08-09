using GymMax.Domain.Entities;
using GymMax.Models;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opciones)
            : base(opciones)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Sede> Sedes { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Suscripcion> Suscripciones { get; set; }
        public DbSet<Plan> Planes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Tablas
            modelBuilder.Entity<Usuario>()
                .ToTable("Usuario");

            modelBuilder.Entity<Rol>()
                .ToTable("Rol");

            modelBuilder.Entity<Sede>()
                .ToTable("Sede");

            modelBuilder.Entity<Asistencia>()
                .ToTable("Asistencia");

            modelBuilder.Entity<Pago>()
                .ToTable("Pago");

            modelBuilder.Entity<Suscripcion>()
                .ToTable("Suscripcion");

            modelBuilder.Entity<Plan>()
                .ToTable("Planes");


            // Relaciones

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany()
                .HasForeignKey(u => u.RolId);

            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId);


            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Sede)
                .WithMany()
                .HasForeignKey(a => a.SedeId);


            modelBuilder.Entity<Suscripcion>()
                .HasOne(s => s.Usuario)
                .WithMany()
                .HasForeignKey(s => s.UsuarioId);


            modelBuilder.Entity<Suscripcion>()
                .HasOne(s => s.Plan)
                .WithMany()
                .HasForeignKey(s => s.PlanId);


            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Suscripcion)
                .WithMany()
                .HasForeignKey(p => p.SuscripcionId);
        }
    }
}