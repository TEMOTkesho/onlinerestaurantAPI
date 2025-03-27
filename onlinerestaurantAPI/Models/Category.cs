namespace OnlineRestaurantAPI.Models
{
    public enum CategoryType
    {
        Salads,
        Soups,
        ChickenDishes,
        BeefDishes,
        SeafoodDishes,
        VegetableDishes,
        BitsAndBites,
        OnTheSide
    }

    public class Category
    {
        public int Id { get; set; }
        public CategoryType Name { get; set; }
        public List<Product>? Products { get; set; }
    }
}
