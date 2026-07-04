using ECommerce.API.DTOs;
using ECommerce.API.Helpers;
using ECommerce.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var result = await _orderService.CreateFromCartAsync(userId, dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var orders = await _orderService.GetMyOrdersAsync(userId);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var order = await _orderService.GetByIdAsync(userId, id);
        if (order is null)
            return NotFound(new { message = "Sipariş bulunamadı." });

        return Ok(order);
    }
}
