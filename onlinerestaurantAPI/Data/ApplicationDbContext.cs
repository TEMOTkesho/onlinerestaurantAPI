using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineRestaurantAPI.Models;

namespace OnlineRestaurantAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

           
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

           
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = CategoryType.Salads },
                new Category { Id = 2, Name = CategoryType.Soups },
                new Category { Id = 3, Name = CategoryType.ChickenDishes },
                new Category { Id = 4, Name = CategoryType.BeefDishes },
                new Category { Id = 5, Name = CategoryType.SeafoodDishes },
                new Category { Id = 6, Name = CategoryType.VegetableDishes },
                new Category { Id = 7, Name = CategoryType.BitsAndBites },
                new Category { Id = 8, Name = CategoryType.OnTheSide }
            );

           
            builder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Caesar Salad",
                    Price = 12.99M,
                    ContainsNuts = false,
                    ImageUrl = "https://example.com/caesar-salad.jpg",
                    IsVegetarian = true,
                    Spiciness = 0,
                    CategoryId = 1
                },
                new Product
                {
                    Id = 2,
                    Name = "Hot and Sour Soup",
                    Price = 8.99M,
                    ContainsNuts = false,
                    ImageUrl = "https://example.com/hot-sour-soup.jpg",
                    IsVegetarian = false,
                    Spiciness = 2,
                    CategoryId = 2
                },
                new Product
                {
                    Id = 3,
                    Name = "Kung Pao Chicken",
                    Price = 15.99M,
                    ContainsNuts = true,
                    ImageUrl = "https://example.com/kung-pao-chicken.jpg",
                    IsVegetarian = false,
                    Spiciness = 3,
                    CategoryId = 3
                },
                new Product
                {
                    Id = 4,
                    Name = "Beef Stir Fry",
                    Price = 16.99M,
                    ContainsNuts = false,
                    ImageUrl = "https://example.com/beef-stir-fry.jpg",
                    IsVegetarian = false,
                    Spiciness = 1,
                    CategoryId = 4
                }
            );
        }
    }
}
