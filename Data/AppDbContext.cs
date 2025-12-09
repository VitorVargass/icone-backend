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
        public DbSet<IngredientModel> Ingredients { get; set; }
        public DbSet<Neutral> Neutrals { get; set; }
        public DbSet<AdditiveModel> Additives { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            

            modelBuilder.HasDefaultSchema("public");

            
            modelBuilder.Entity<AdminModel>().ToTable("admins");
            modelBuilder.Entity<UserModel>().ToTable("users");
            modelBuilder.Entity<CompaniesModel>().ToTable("companies");
            modelBuilder.Entity<IngredientModel>().ToTable("ingredients");
            modelBuilder.Entity<Neutral>().ToTable("neutrals");
            modelBuilder.Entity<AdditiveModel>().ToTable("additives");



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

            modelBuilder.Entity<IngredientModel>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
            modelBuilder.Entity<AdditiveModel>()
                .OwnsOne(a => a.Scores);

            base.OnModelCreating(modelBuilder);
        }
    }
}
