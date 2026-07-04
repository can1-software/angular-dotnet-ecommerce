using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, OrderDto? Data)> CreateFromCartAsync(int userId, CreateOrderDto dto)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null || cart.Items.Count == 0)
            return (false, "Sepetiniz boş.", null);

        foreach (var item in cart.Items)
        {
            if (item.Quantity > item.Product.Stock)
                return (false, $"{item.Product.Name} için stok yetersiz.", null);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                UserId = userId,
                ShippingFullName = dto.ShippingFullName.Trim(),
                ShippingPhone = dto.ShippingPhone.Trim(),
                ShippingAddress = dto.ShippingAddress.Trim(),
                ShippingCity = dto.ShippingCity.Trim(),
                Status = OrderStatus.Pending
            };

            decimal total = 0;
            foreach (var item in cart.Items)
            {
                var lineTotal = item.Product.Price * item.Quantity;
                total += lineTotal;

                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductImageUrl = item.Product.ImageUrl,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal
                });

                item.Product.Stock -= item.Quantity;
            }

            order.TotalAmount = total;
            order.OrderNumber = $"NS-{DateTime.UtcNow:yyyyMMddHHmmss}-{userId}";
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            order.OrderNumber = $"NS-{DateTime.UtcNow:yyyyMMdd}-{order.Id:D4}";
            await _context.SaveChangesAsync();

            _context.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return (true, "Siparişiniz alındı.", ToDto(order));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Sipariş oluşturulamadı: {ex.InnerException?.Message ?? ex.Message}", null);
        }
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync(int userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(int userId, int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order is null ? null : ToDto(order);
    }

    private static OrderDto ToDto(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        ShippingFullName = order.ShippingFullName,
        ShippingPhone = order.ShippingPhone,
        ShippingAddress = order.ShippingAddress,
        ShippingCity = order.ShippingCity,
        CreatedAt = order.CreatedAt,
        Items = order.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            ProductImageUrl = i.ProductImageUrl,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal
        }).ToList()
    };
}
