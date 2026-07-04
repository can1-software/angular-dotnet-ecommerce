using ECommerce.API.DTOs;
using ECommerce.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _productService.GetPagedAsync(search, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
            return NotFound(new { message = "Ürün bulunamadı." });

        return Ok(product);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateProductDto dto, IFormFile? image)
    {
        var result = await _productService.CreateAsync(dto, image);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto dto, IFormFile? image)
    {
        var result = await _productService.UpdateAsync(id, dto, image);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(new { message = result.Message });
    }
}
