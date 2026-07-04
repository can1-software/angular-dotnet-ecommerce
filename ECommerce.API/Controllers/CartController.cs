using ECommerce.API.DTOs;
using ECommerce.API.Helpers;
using ECommerce.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddToCartDto dto)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var result = await _cartService.AddItemAsync(userId, dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateItem(int cartItemId, UpdateCartItemDto dto)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var result = await _cartService.UpdateItemAsync(userId, cartItemId, dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var result = await _cartService.RemoveItemAsync(userId, cartItemId);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }
}
