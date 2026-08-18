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
        public DbSet<Conversacion> Conversaciones { get; set; }
        public DbSet<ConversacionMiembro> ConversacionMiembros { get; set; }
        public DbSet<Mensaje> Mensajes { get; set; }
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
                .WithMany(u => u.Suscripciones)
                .HasForeignKey(s => s.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Suscripcion>()
                .HasOne(s => s.Plan)
                .WithMany()
                .HasForeignKey(s => s.PlanId);
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Suscripcion)
                .WithMany()
                .HasForeignKey(p => p.SuscripcionId);

            // RELACIONES ENTRE LAS TABLAS DE LA CONVERSACION
            modelBuilder.Entity<Conversacion>(entity =>
            {
                entity.HasKey(c => c.ConversacionId);
                entity.Property(c => c.Nombre)
                    .HasMaxLength(100);
                entity.Property(c => c.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.Activa)
                    .HasDefaultValue(true);
                entity.Property(c => c.Tipo)
                    .HasConversion<int>();
                // Usuario que creó la conversación
                entity.HasOne(c => c.CreadaPorUsuario)
                    .WithMany()
                    .HasForeignKey(c => c.CreadaPorUsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // CONVERSACION MIEMBRO
            modelBuilder.Entity<ConversacionMiembro>(entity =>
            {
                entity.HasKey(cm => cm.ConversacionMiembroId);
                entity.Property(cm => cm.FechaIngreso)
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(cm => cm.Activo)
                    .HasDefaultValue(true);
                // Conversación
                entity.HasOne(cm => cm.Conversacion)
                    .WithMany(c => c.Miembros)
                    .HasForeignKey(cm => cm.ConversacionId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Usuario
                entity.HasOne(cm => cm.Usuario)
                    .WithMany()
                    .HasForeignKey(cm => cm.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
                // Evitar que un usuario aparezca dos veces
                // en la misma conversación
                entity.HasIndex(cm => new
                {
                    cm.ConversacionId,
                    cm.UsuarioId
                })
                .IsUnique();
            });
            // MENSAJE
            modelBuilder.Entity<Mensaje>(entity =>
            {
                entity.HasKey(m => m.MensajeId);
                entity.Property(m => m.Contenido)
                    .IsRequired();
                entity.Property(m => m.FechaEnvio)
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(m => m.Editado)
                    .HasDefaultValue(false);
                entity.Property(m => m.Eliminado)
                    .HasDefaultValue(false);
                // Conversación
                entity.HasOne(m => m.Conversacion)
                    .WithMany(c => c.Mensajes)
                    .HasForeignKey(m => m.ConversacionId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Usuario que envió el mensaje
                entity.HasOne(m => m.Usuario)
                    .WithMany()
                    .HasForeignKey(m => m.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
                // Índice para obtener rápidamente
                // los mensajes de una conversación
                entity.HasIndex(m => new
                {
                    m.ConversacionId,
                    m.FechaEnvio
                });
            });
        }
    }
}