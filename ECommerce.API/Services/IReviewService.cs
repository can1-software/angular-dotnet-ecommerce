using ECommerce.API.DTOs;

namespace ECommerce.API.Services;

public interface IReviewService
{
    Task<ProductReviewSummaryDto> GetByProductSlugAsync(string productSlug, int? currentUserId);
    Task<(bool Success, string Message, ReviewDto? Data)> CreateAsync(int userId, CreateReviewDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int userId, string userRole, int reviewId);
}
