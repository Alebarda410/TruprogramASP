using Microsoft.EntityFrameworkCore;

namespace Truprogram.Models
{
    public sealed class DataBaseContext : DbContext
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UsersCourses> UsersCourses { get; set; }
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}