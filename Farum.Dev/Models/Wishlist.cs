namespace LittleBugShop.Models
{
    public class Wishlist
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<int> ProductIds { get; set; } = new List<int>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class WishlistItem
    {
        public int ProductId { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
