using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OnlineRestaurantAPI.Models;
using OnlineRestaurantAPI.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OnlineRestaurantAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BasketController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BasketItem>>> GetBasketItems()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return await _context.BasketItems
                .Include(b => b.Product)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<BasketItem>> AddToBasket([FromBody] BasketItem item)
        {
            if (item == null || item.ProductId == 0 || item.Quantity <= 0)
                return BadRequest("Invalid item details");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            item.UserId = userId;
            
            var existingItem = await _context.BasketItems
                .FirstOrDefaultAsync(b => b.UserId == userId && b.ProductId == item.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    return NotFound("Product not found");
                }

                _context.BasketItems.Add(item);
            }

            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int quantity)
        {
            if (quantity <= 0)
                return BadRequest("Quantity must be greater than 0");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var basketItem = await _context.BasketItems
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (basketItem == null)
            {
                return NotFound();
            }

            basketItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromBasket(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var basketItem = await _context.BasketItems
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (basketItem == null)
            {
                return NotFound();
            }

            _context.BasketItems.Remove(basketItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
