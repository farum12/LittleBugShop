namespace LittleBugShop.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public string? AppliedCouponCode { get; set; }
        public decimal Subtotal => Items.Sum(item => item.TotalPrice);
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice => Subtotal - DiscountAmount;
        public int TotalItems => Items.Sum(item => item.Quantity);
        public DateTime LastUpdated { get; set; }
    }
}
