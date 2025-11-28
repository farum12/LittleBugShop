using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LittleBugShop.Data;
using LittleBugShop.Models;
using System.Security.Claims;

namespace LittleBugShop.Controllers
{
    [Route("api/products/{productId}/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        // Create or update review
        [HttpPost]
        [Authorize]
        public ActionResult<Review> CreateOrUpdateReview(int productId, [FromBody] CreateReviewRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            
            if (userIdClaim == null || userNameClaim == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var userId = int.Parse(userIdClaim);

            // Validate product exists
            var product = Database.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Validate rating
            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            // Check if user has purchased this product
            var hasPurchased = Database.Orders
                .Where(o => o.UserId == userId)
                .Any(o => o.Items.Any(i => i.ProductId == productId));

            // Check if user already has a review for this product
            var existingReview = Database.Reviews.FirstOrDefault(r => r.ProductId == productId && r.UserId == userId);

            if (existingReview != null)
            {
                // Update existing review
                existingReview.Rating = request.Rating;
                existingReview.ReviewText = request.ReviewText;
                existingReview.UpdatedAt = DateTime.UtcNow;
                existingReview.IsVerifiedPurchase = hasPurchased;
                
                return Ok(existingReview);
            }
            else
            {
                // Create new review
                var review = new Review
                {
                    Id = Database.Reviews.Any() ? Database.Reviews.Max(r => r.Id) + 1 : 1,
                    ProductId = productId,
                    UserId = userId,
                    UserName = userNameClaim,
                    Rating = request.Rating,
                    ReviewText = request.ReviewText,
                    IsVerifiedPurchase = hasPurchased,
                    HelpfulCount = 0,
                    IsHidden = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                Database.Reviews.Add(review);
                return CreatedAtAction(nameof(GetReview), new { productId = productId, reviewId = review.Id }, review);
            }
        }

        // Get all reviews for a product
        [HttpGet]
        public ActionResult<object> GetReviews(
            int productId,
            [FromQuery] int? rating = null,
            [FromQuery] bool? verifiedOnly = null,
            [FromQuery] string sortBy = "date",
            [FromQuery] string sortOrder = "desc")
        {
            var product = Database.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var reviews = Database.Reviews
                .Where(r => r.ProductId == productId && !r.IsHidden)
                .ToList();

            // Filter by rating
            if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
            {
                reviews = reviews.Where(r => r.Rating == rating.Value).ToList();
            }

            // Filter verified purchases only
            if (verifiedOnly.HasValue && verifiedOnly.Value)
            {
                reviews = reviews.Where(r => r.IsVerifiedPurchase).ToList();
            }

            // Sort reviews
            reviews = sortBy.ToLower() switch
            {
                "rating" => sortOrder.ToLower() == "asc" 
                    ? reviews.OrderBy(r => r.Rating).ToList() 
                    : reviews.OrderByDescending(r => r.Rating).ToList(),
                "helpful" => sortOrder.ToLower() == "asc" 
                    ? reviews.OrderBy(r => r.HelpfulCount).ToList() 
                    : reviews.OrderByDescending(r => r.HelpfulCount).ToList(),
                _ => sortOrder.ToLower() == "asc" 
                    ? reviews.OrderBy(r => r.CreatedAt).ToList() 
                    : reviews.OrderByDescending(r => r.CreatedAt).ToList()
            };

            // Check if current user marked each review as helpful
            int? currentUserId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null)
            {
                currentUserId = int.Parse(userIdClaim);
            }

            var reviewsWithHelpful = reviews.Select(r => new
            {
                r.Id,
                r.ProductId,
                r.UserId,
                r.UserName,
                r.Rating,
                r.ReviewText,
                r.IsVerifiedPurchase,
                r.HelpfulCount,
                r.CreatedAt,
                r.UpdatedAt,
                MarkedAsHelpfulByCurrentUser = currentUserId.HasValue 
                    ? Database.ReviewHelpfuls.Any(rh => rh.ReviewId == r.Id && rh.UserId == currentUserId.Value)
                    : false
            });

            return Ok(new
            {
                productId,
                productName = product.Name,
                averageRating = product.AverageRating,
                totalReviews = reviews.Count,
                reviews = reviewsWithHelpful
            });
        }

        // Get specific review
        [HttpGet("{reviewId}")]
        public ActionResult<Review> GetReview(int productId, int reviewId)
        {
            var review = Database.Reviews.FirstOrDefault(r => r.Id == reviewId && r.ProductId == productId);
            if (review == null)
            {
                return NotFound("Review not found.");
            }

            if (review.IsHidden)
            {
                // Only admin or the review author can see hidden reviews
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                if (userRole != "Admin" && (userIdClaim == null || int.Parse(userIdClaim) != review.UserId))
                {
                    return NotFound("Review not found.");
                }
            }

            return Ok(review);
        }

        // Get current user's review for a product
        [HttpGet("~/api/products/{productId}/my-review")]
        [Authorize]
        public ActionResult<Review> GetMyReview(int productId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var userId = int.Parse(userIdClaim);
            var review = Database.Reviews.FirstOrDefault(r => r.ProductId == productId && r.UserId == userId);

            if (review == null)
            {
                return NotFound("You haven't reviewed this product yet.");
            }

            return Ok(review);
        }

        // Delete review
        [HttpDelete("{reviewId}")]
        [Authorize]
        public IActionResult DeleteReview(int productId, int reviewId)
        {
            var review = Database.Reviews.FirstOrDefault(r => r.Id == reviewId && r.ProductId == productId);
            if (review == null)
            {
                return NotFound("Review not found.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var userId = int.Parse(userIdClaim);

            // Only allow deletion if user is the author or admin
            if (review.UserId != userId && userRole != "Admin")
            {
                return Forbid("You can only delete your own reviews.");
            }

            // Remove associated helpful marks
            var helpfulMarks = Database.ReviewHelpfuls.Where(rh => rh.ReviewId == reviewId).ToList();
            foreach (var mark in helpfulMarks)
            {
                Database.ReviewHelpfuls.Remove(mark);
            }

            Database.Reviews.Remove(review);
            return NoContent();
        }

        // Mark review as helpful
        [HttpPost("~/api/reviews/{reviewId}/helpful")]
        [Authorize]
        public ActionResult<object> MarkReviewHelpful(int reviewId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var userId = int.Parse(userIdClaim);

            var review = Database.Reviews.FirstOrDefault(r => r.Id == reviewId);
            if (review == null)
            {
                return NotFound("Review not found.");
            }

            // Check if user already marked this review as helpful
            var existingMark = Database.ReviewHelpfuls.FirstOrDefault(rh => rh.ReviewId == reviewId && rh.UserId == userId);

            if (existingMark != null)
            {
                // Remove the helpful mark (toggle off)
                Database.ReviewHelpfuls.Remove(existingMark);
                review.HelpfulCount--;
                
                return Ok(new { message = "Removed helpful mark", helpfulCount = review.HelpfulCount, markedAsHelpful = false });
            }
            else
            {
                // Add helpful mark
                var helpfulMark = new ReviewHelpful
                {
                    Id = Database.ReviewHelpfuls.Any() ? Database.ReviewHelpfuls.Max(rh => rh.Id) + 1 : 1,
                    ReviewId = reviewId,
                    UserId = userId,
                    MarkedAt = DateTime.UtcNow
                };

                Database.ReviewHelpfuls.Add(helpfulMark);
                review.HelpfulCount++;

                return Ok(new { message = "Marked as helpful", helpfulCount = review.HelpfulCount, markedAsHelpful = true });
            }
        }

        // Admin: Hide/unhide review
        [HttpPut("{reviewId}/moderate")]
        [Authorize(Roles = "Admin")]
        public ActionResult<Review> ModerateReview(int productId, int reviewId, [FromBody] ModerateReviewRequest request)
        {
            var review = Database.Reviews.FirstOrDefault(r => r.Id == reviewId && r.ProductId == productId);
            if (review == null)
            {
                return NotFound("Review not found.");
            }

            review.IsHidden = request.IsHidden;
            return Ok(review);
        }

        // Admin: Get all reviews including hidden ones
        [HttpGet("~/api/admin/reviews")]
        [Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<Review>> GetAllReviews(
            [FromQuery] bool? includeHidden = true,
            [FromQuery] int? productId = null)
        {
            var reviews = Database.Reviews.AsEnumerable();

            if (productId.HasValue)
            {
                reviews = reviews.Where(r => r.ProductId == productId.Value);
            }

            if (!includeHidden.HasValue || !includeHidden.Value)
            {
                reviews = reviews.Where(r => !r.IsHidden);
            }

            return Ok(reviews.OrderByDescending(r => r.CreatedAt));
        }
    }

    public class CreateReviewRequest
    {
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
    }

    public class ModerateReviewRequest
    {
        public bool IsHidden { get; set; }
    }
}
