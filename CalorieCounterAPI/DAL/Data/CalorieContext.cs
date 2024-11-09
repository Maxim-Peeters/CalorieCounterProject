using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class CalorieContext : DbContext
    {
        public CalorieContext(DbContextOptions<CalorieContext> options) : base(options)
        { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Day> Days { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<Day>().ToTable("Day");

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Day)               // A Product has one Day
                .WithMany(d => d.Products);         // A Day has many Products
        }
    }
}
