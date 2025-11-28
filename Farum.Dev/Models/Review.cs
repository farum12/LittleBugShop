namespace LittleBugShop.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5 stars
        public string? ReviewText { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public int HelpfulCount { get; set; }
        public bool IsHidden { get; set; } // For admin moderation
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
