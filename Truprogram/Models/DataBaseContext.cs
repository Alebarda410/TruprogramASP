using Microsoft.EntityFrameworkCore;

namespace Truprogram.Models
{
    public sealed class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UsersCourses> UsersCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // добавляем роли
            var lecturerRole = new Role {Id = 1, Name = "Лектор"};
            var listenerRole = new Role {Id = 2, Name = "Слушатель"};

            modelBuilder.Entity<Role>().HasData(lecturerRole, listenerRole);
            base.OnModelCreating(modelBuilder);
        }
    }
}