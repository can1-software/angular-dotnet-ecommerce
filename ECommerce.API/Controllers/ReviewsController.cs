using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using ECommerce.API.Helpers;
using ECommerce.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [AllowAnonymous]
    [HttpGet("product/{productSlug}")]
    public async Task<IActionResult> GetByProductSlug(string productSlug)
    {
        var userId = TryGetUserId();
        var result = await _reviewService.GetByProductSlugAsync(productSlug, userId);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateReviewDto dto)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var result = await _reviewService.CreateAsync(userId, dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? UserRole.Customer;
        var result = await _reviewService.DeleteAsync(userId, role, id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    private int? TryGetUserId()
    {
        var id = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(id, out var userId) ? userId : null;
    }
}
