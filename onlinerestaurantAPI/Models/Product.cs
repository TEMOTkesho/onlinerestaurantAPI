namespace OnlineRestaurantAPI.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool Nuts { get; set; }
    public string Image { get; set; }
    public bool Vegeterian { get; set; }
    public int Spiciness { get; set; }
    public int CategoryId { get; set; }
}