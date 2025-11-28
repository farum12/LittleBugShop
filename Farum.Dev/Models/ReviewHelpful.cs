namespace LittleBugShop.Models
{
    public class ReviewHelpful
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public DateTime MarkedAt { get; set; }
    }
}
