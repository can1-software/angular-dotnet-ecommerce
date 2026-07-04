using ECommerce.API.DTOs;

namespace ECommerce.API.Services;

public interface IOrderService
{
    Task<(bool Success, string Message, OrderDto? Data)> CreateFromCartAsync(int userId, CreateOrderDto dto);
    Task<List<OrderDto>> GetMyOrdersAsync(int userId);
    Task<OrderDto?> GetByIdAsync(int userId, int orderId);
}
