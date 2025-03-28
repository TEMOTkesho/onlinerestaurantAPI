using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRestaurantAPI.Data;
using OnlineRestaurantAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace OnlineRestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProducts(
            [FromQuery] CategoryType? category,
            [FromQuery] int? minSpiciness,
            [FromQuery] int? maxSpiciness,
            [FromQuery] bool? containsNuts,
            [FromQuery] bool? isVegetarian)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (category.HasValue)
            {
                query = query.Where(p => p.Category.Name == category.Value);
            }

            if (minSpiciness.HasValue)
            {
                query = query.Where(p => p.Spiciness >= minSpiciness.Value);
            }

            if (maxSpiciness.HasValue)
            {
                query = query.Where(p => p.Spiciness <= maxSpiciness.Value);
            }

            if (containsNuts.HasValue)
            {
                query = query.Where(p => p.ContainsNuts == containsNuts.Value);
            }

            if (isVegetarian.HasValue)
            {
                query = query.Where(p => p.IsVegetarian == isVegetarian.Value);
            }

            var products = await query.Select(p => new
            {
                p.Id,
                p.Name,
                p.Spiciness,
                p.ContainsNuts,
                p.IsVegetarian,
                p.Price,
                p.ImageUrl,
                p.CategoryId,
                Category = new
                {
                    p.Category.Id,
                    p.Category.Name
                }
            }).ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpGet("categories")]
        public ActionResult<IEnumerable<CategoryType>> GetCategories()
        {
            return Enum.GetValues(typeof(CategoryType)).Cast<CategoryType>().ToList();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Invalid product data.");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("Product ID mismatch.");
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.ContainsNuts = product.ContainsNuts;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.IsVegetarian = product.IsVegetarian;
            existingProduct.Spiciness = product.Spiciness;
            existingProduct.CategoryId = product.CategoryId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
