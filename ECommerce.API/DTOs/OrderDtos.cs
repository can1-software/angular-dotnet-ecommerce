using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

public class CreateOrderDto
{
    [Required(ErrorMessage = "Alıcı adı zorunludur.")]
    [MaxLength(200)]
    public string ShippingFullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon zorunludur.")]
    [MaxLength(20)]
    public string ShippingPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres zorunludur.")]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şehir zorunludur.")]
    [MaxLength(100)]
    public string ShippingCity { get; set; } = string.Empty;
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
