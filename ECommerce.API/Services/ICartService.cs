using ECommerce.API.DTOs;

namespace ECommerce.API.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<(bool Success, string Message, CartDto? Data)> AddItemAsync(int userId, AddToCartDto dto);
    Task<(bool Success, string Message, CartDto? Data)> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto);
    Task<(bool Success, string Message, CartDto? Data)> RemoveItemAsync(int userId, int cartItemId);
    Task ClearCartAsync(int userId);
}
