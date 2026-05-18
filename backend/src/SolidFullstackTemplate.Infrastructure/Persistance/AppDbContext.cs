using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Infrastructure.Persistance;

internal class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User>(options)
{
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Dish> Dishes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Restaurant>()
            .OwnsOne(restaurant => restaurant.Address);

        modelBuilder.Entity<Restaurant>()
            .HasMany(r => r.Dishes)
            .WithOne()
            .HasForeignKey(d => d.RestaurantId);

        modelBuilder.Entity<User>()
            .HasMany(u => u.OwnedRestaurants)
            .WithOne(r => r.Owner)
            .HasForeignKey(r => r.OwnerId);
    }
}
