using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return ToDto(cart);
    }

    public async Task<(bool Success, string Message, CartDto? Data)> AddItemAsync(int userId, AddToCartDto dto)
    {
        if (dto.Quantity < 1)
            return (false, "Adet en az 1 olmalıdır.", null);

        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product is null)
            return (false, "Ürün bulunamadı.", null);

        if (product.Stock < 1)
            return (false, "Ürün stokta yok.", null);

        var cart = await GetOrCreateCartAsync(userId);
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

        var newQty = (existing?.Quantity ?? 0) + dto.Quantity;
        if (newQty > product.Stock)
            return (false, $"Stok yetersiz. Maksimum {product.Stock} adet eklenebilir.", null);

        if (existing is not null)
            existing.Quantity = newQty;
        else
            cart.Items.Add(new CartItem { ProductId = dto.ProductId, Quantity = dto.Quantity });

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await ReloadCartItems(cart);
        return (true, "Ürün sepete eklendi.", ToDto(cart));
    }

    public async Task<(bool Success, string Message, CartDto? Data)> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto)
    {
        if (dto.Quantity < 1)
            return (false, "Adet en az 1 olmalıdır.", null);

        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null)
            return (false, "Sepet kalemi bulunamadı.", null);

        if (dto.Quantity > item.Product.Stock)
            return (false, $"Stok yetersiz. Maksimum {item.Product.Stock} adet.", null);

        item.Quantity = dto.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Sepet güncellendi.", ToDto(cart));
    }

    public async Task<(bool Success, string Message, CartDto? Data)> RemoveItemAsync(int userId, int cartItemId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null)
            return (false, "Sepet kalemi bulunamadı.", null);

        _context.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await ReloadCartItems(cart);
        return (true, "Ürün sepetten çıkarıldı.", ToDto(cart));
    }

    public async Task ClearCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null) return;

        _context.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is not null) return cart;

        cart = new Cart { UserId = userId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        return await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstAsync(c => c.Id == cart.Id);
    }

    private async Task ReloadCartItems(Cart cart)
    {
        await _context.Entry(cart).Collection(c => c.Items).Query()
            .Include(i => i.Product)
            .LoadAsync();
    }

    private static CartDto ToDto(Cart cart)
    {
        var items = cart.Items.Select(i => new CartItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product.Name,
            ProductSlug = i.Product.Slug,
            ProductImageUrl = i.Product.ImageUrl,
            UnitPrice = i.Product.Price,
            Quantity = i.Quantity,
            Stock = i.Product.Stock,
            LineTotal = i.Product.Price * i.Quantity
        }).ToList();

        return new CartDto
        {
            Id = cart.Id,
            Items = items,
            TotalItems = items.Sum(i => i.Quantity),
            TotalAmount = items.Sum(i => i.LineTotal)
        };
    }
}
