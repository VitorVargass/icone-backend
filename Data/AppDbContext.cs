using icone_backend.Models;
using Microsoft.EntityFrameworkCore;


namespace icone_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) { }

        public DbSet<AdminModel> Admins { get; set; }
        public DbSet<UserModel> Users { get; set; }
    
        public DbSet<CompaniesModel> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.HasKey(e => e.Id);        
            });

            modelBuilder.Entity<AdminModel>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<CompaniesModel>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}
