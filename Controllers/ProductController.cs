using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_auth.Models;
using System.Security.Claims;
using dotnet_auth.Services;

namespace dotnet_auth.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        product.UserId = userId;

        var createdProduct = await _productService.CreateProductAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        if (id != product.Id)
            return BadRequest();

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var existingProduct = await _productService.GetProductByIdAsync(id);

        if (existingProduct == null)
            return NotFound();

        if (existingProduct.UserId != userId)
            return Forbid();

        var updatedProduct = await _productService.UpdateProductAsync(product);
        return Ok(updatedProduct);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
            return NotFound();

        if (product.UserId != userId)
            return Forbid();

        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
}