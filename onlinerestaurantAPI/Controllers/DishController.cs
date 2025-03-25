using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRestaurantAPI.Data;
using OnlineRestaurantAPI.Models;
using System.Threading.Tasks;

namespace OnlineRestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DishController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DishController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost("add")]
        public async Task<ActionResult<Product>> AddDish([FromBody] Product dish)
        {
            if (dish == null)
            {
                return BadRequest("Invalid dish data.");
            }


            if (string.IsNullOrEmpty(dish.Name) || dish.Price <= 0)
            {
                return BadRequest("Invalid dish name or price.");
            }


            _context.Products.Add(dish);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDishById), new { id = dish.Id }, dish);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetDishById(int id)
        {
            var dish = await _context.Products.FindAsync(id);

            if (dish == null)
            {
                return NotFound("Dish not found.");
            }

            return Ok(dish);
        }
    }
}
