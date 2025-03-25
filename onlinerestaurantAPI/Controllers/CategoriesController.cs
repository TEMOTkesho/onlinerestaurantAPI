using Microsoft.AspNetCore.Mvc;
using OnlineRestaurantAPI.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private const string BASE_URL = "https://localhost:7017";

    public CategoriesController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetAllCategories()
    {

        var response = await _httpClient.GetStringAsync($"{BASE_URL}/api/Categories/GetAll");
        var categories = JsonSerializer.Deserialize<List<Category>>(response,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return Ok(categories);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetCategoryById(int id)
    {

        var response = await _httpClient.GetStringAsync($"{BASE_URL}/api/Categories/GetCategory/{id}");
        var category = JsonSerializer.Deserialize<Category>(response,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (category == null)
            return NotFound();

        return Ok(category);
    }
}
