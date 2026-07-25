using GymMax.Domain.Entities;
using GymMax.Models;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Data {
    public class AppDbContext : DbContext {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { 
        }


        // DbSets
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Plan> Planes => Set<Plan>();
        public DbSet<Suscripcion> Suscripciones => Set<Suscripcion>();
        public DbSet<Pago> Pagos => Set<Pago>();
        public DbSet<Sede> Sedes => Set<Sede>();
        public DbSet<Coach> Coaches => Set<Coach>();
        public DbSet<Asistencia> Asistencias => Set<Asistencia>();

    }
}
