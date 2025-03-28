using System.ComponentModel.DataAnnotations;

namespace OnlineRestaurantAPI.Models
{
    public class AddToBasketRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
} 