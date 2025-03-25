using Microsoft.AspNetCore.Mvc;
using OnlineRestaurantAPI.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class BasketsController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private const string BASE_URL = "https://localhost:7017"; 
    public BasketsController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    [HttpGet]
    public async Task<ActionResult<List<BasketItem>>> GetAllBasketItems()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{BASE_URL}/api/Baskets/GetAll");

            if (string.IsNullOrEmpty(response))
                return NotFound("Basket is empty");

            var basketItems = JsonSerializer.Deserialize<List<BasketItem>>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(basketItems);
        }
        catch (HttpRequestException)
        {
            return StatusCode(500, "Error occurred while fetching basket items");
        }
    }


    [HttpPost]
    public async Task<ActionResult> AddToBasket([FromBody] BasketItem item)
    {
        if (item == null || item.ProductId == 0 || item.Quantity <= 0)
            return BadRequest("Invalid item details");

        try
        {
            var json = JsonSerializer.Serialize(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BASE_URL}/api/Baskets/AddToBasket", content);
            response.EnsureSuccessStatusCode();

            return Ok("Item added to basket");
        }
        catch (HttpRequestException)
        {
            return StatusCode(500, "Error occurred while adding item to basket");
        }
    }


    [HttpPut]
    public async Task<ActionResult> UpdateBasket([FromBody] BasketItem item)
    {
        if (item == null || item.ProductId == 0 || item.Quantity <= 0)
            return BadRequest("Invalid item details");

        try
        {
            var json = JsonSerializer.Serialize(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{BASE_URL}/api/Baskets/UpdateBasket", content);
            response.EnsureSuccessStatusCode();

            return Ok("Basket updated");
        }
        catch (HttpRequestException)
        {
            return StatusCode(500, "Error occurred while updating the basket");
        }
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProductFromBasket(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid product ID");

        try
        {
            var response = await _httpClient.DeleteAsync($"{BASE_URL}/api/Baskets/DeleteProduct/{id}");
            response.EnsureSuccessStatusCode();

            return Ok($"Product with id {id} deleted from basket");
        }
        catch (HttpRequestException)
        {
            return StatusCode(500, "Error occurred while deleting product from basket");
        }
    }
}
