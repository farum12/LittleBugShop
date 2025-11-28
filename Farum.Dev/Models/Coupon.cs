namespace LittleBugShop.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DiscountType Type { get; set; }
        public decimal Value { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? MaxUsesTotal { get; set; }
        public bool IsActive { get; set; }
        public int CurrentUses { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum DiscountType
    {
        Percentage = 0,
        FixedAmount = 1
    }

    public class CouponUsage
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public DateTime UsedAt { get; set; }
    }
}
