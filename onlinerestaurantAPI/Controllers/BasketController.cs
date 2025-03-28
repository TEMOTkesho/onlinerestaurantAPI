using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OnlineRestaurantAPI.Models;
using OnlineRestaurantAPI.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Linq;
using System.Security.Claims;

namespace OnlineRestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/basket")]
    public class BasketController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BasketController> _logger;

        public BasketController(ApplicationDbContext context, ILogger<BasketController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            _logger.LogInformation("Test endpoint called");
            return Ok("Basket controller is working!");
        }

        [HttpGet("items")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BasketItem>>> GetBasketItems()
        {
            try
            {
                _logger.LogInformation("GetBasketItems endpoint called");
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"User ID from token: {userId}");

                if (userId == null)
                {
                    _logger.LogWarning("Unauthorized access attempt to GetBasketItems");
                    return Unauthorized();
                }

                var items = await _context.BasketItems
                    .Include(b => b.Product)
                    .Where(b => b.UserId == userId)
                    .ToListAsync();

                _logger.LogInformation($"Found {items.Count} items in basket for user {userId}");
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBasketItems");
                return StatusCode(500, "An error occurred while retrieving basket items");
            }
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddToBasket([FromBody] AddToBasketRequest request)
        {
            try
            {
                _logger.LogInformation("AddToBasket called");
                var authHeader = Request.Headers["Authorization"].ToString();
                _logger.LogInformation($"Auth header present: {!string.IsNullOrEmpty(authHeader)}");
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"User ID from token: {userId ?? "not found"}");
                
                if (userId == null)
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                if (request == null)
                {
                    _logger.LogWarning("Request body is null");
                    return BadRequest(new { message = "Request body is required" });
                }

                _logger.LogInformation($"Processing request for user {userId}, product {request.ProductId}, quantity {request.Quantity}");

                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    _logger.LogWarning($"Product {request.ProductId} not found");
                    return NotFound(new { message = $"Product {request.ProductId} not found" });
                }

                var basketItem = await _context.BasketItems
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.ProductId == request.ProductId);

                if (basketItem != null)
                {
                    basketItem.Quantity += request.Quantity;
                    _logger.LogInformation($"Updated quantity for existing basket item to {basketItem.Quantity}");
                }
                else
                {
                    basketItem = new BasketItem
                    {
                        UserId = userId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity
                    };
                    await _context.BasketItems.AddAsync(basketItem);
                    _logger.LogInformation("Created new basket item");
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Changes saved to database");

                return Ok(new { message = "Item added to basket", item = basketItem });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddToBasket");
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int quantity)
        {
            if (quantity <= 0)
                return BadRequest("Quantity must be greater than 0");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var basketItem = await _context.BasketItems
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (basketItem == null)
            {
                return NotFound($"Basket item with ID {id} not found");
            }

            basketItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            return Ok(basketItem);
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> RemoveFromBasket(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var basketItem = await _context.BasketItems
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (basketItem == null)
            {
                return NotFound($"Basket item with ID {id} not found");
            }

            _context.BasketItems.Remove(basketItem);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
