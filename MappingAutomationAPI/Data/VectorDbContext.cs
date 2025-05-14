using Microsoft.EntityFrameworkCore;
using MappingAutomationAPI.Models;

namespace MappingAutomationAPI.Data
{
    public class VectorDbContext : DbContext
    {
        public VectorDbContext(DbContextOptions<VectorDbContext> options)
            : base(options)
        { }

        public DbSet<TestVectorEntity> TestVectors { get; set; } = default!;
        public DbSet<SimilarTest> SimilarTests { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<TestVectorEntity>()
                .Property(x => x.Embedding)
                .HasColumnType("vector(1536)");

            modelBuilder.Entity<TestVectorEntity>()
                .HasIndex(e => new { e.Module, e.App, e.TestName })
                .IsUnique();

            modelBuilder.Entity<SimilarTest>()
                .HasNoKey()
                .ToView(null);
        }
    }
}
