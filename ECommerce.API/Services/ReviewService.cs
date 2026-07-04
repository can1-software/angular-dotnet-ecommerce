using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductReviewSummaryDto> GetByProductSlugAsync(string productSlug, int? currentUserId)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == productSlug);

        if (product is null)
            return new ProductReviewSummaryDto();

        var reviews = await _context.ProductReviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.ProductId == product.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return new ProductReviewSummaryDto
        {
            AverageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 1),
            TotalCount = reviews.Count,
            Items = reviews.Select(r => ToDto(r, currentUserId)).ToList()
        };
    }

    public async Task<(bool Success, string Message, ReviewDto? Data)> CreateAsync(int userId, CreateReviewDto dto)
    {
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product is null)
            return (false, "Ürün bulunamadı.", null);

        var alreadyReviewed = await _context.ProductReviews
            .AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);

        if (alreadyReviewed)
            return (false, "Bu ürüne zaten yorum yaptınız.", null);

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
            return (false, "Kullanıcı bulunamadı.", null);

        var review = new ProductReview
        {
            ProductId = dto.ProductId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment.Trim()
        };

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync();

        await _context.Entry(review).Reference(r => r.User).LoadAsync();
        return (true, "Yorumunuz eklendi.", ToDto(review, userId));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int userId, string userRole, int reviewId)
    {
        var review = await _context.ProductReviews.FindAsync(reviewId);
        if (review is null)
            return (false, "Yorum bulunamadı.");

        var isAdmin = userRole == UserRole.Admin;
        if (review.UserId != userId && !isAdmin)
            return (false, "Bu yorumu silme yetkiniz yok.");

        _context.ProductReviews.Remove(review);
        await _context.SaveChangesAsync();
        return (true, "Yorum silindi.");
    }

    private static ReviewDto ToDto(ProductReview review, int? currentUserId) => new()
    {
        Id = review.Id,
        ProductId = review.ProductId,
        UserName = review.User?.FullName ?? "Kullanıcı",
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt,
        IsOwnReview = currentUserId.HasValue && review.UserId == currentUserId.Value
    };
}
