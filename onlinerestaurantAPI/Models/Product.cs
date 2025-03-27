using System.ComponentModel.DataAnnotations;

namespace OnlineRestaurantAPI.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [Range(0, 4)]
    public int Spiciness { get; set; }

    [Required]
    public bool ContainsNuts { get; set; }

    [Required]
    public bool IsVegetarian { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    public string ImageUrl { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}