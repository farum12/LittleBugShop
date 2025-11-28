namespace LittleBugShop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        // Payment information
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public string? TransactionId { get; set; }
        public int? PaymentMethodId { get; set; }
        
        // Shipping information
        public int? ShippingAddressId { get; set; }
        
        // Order expiration (for pending payments)
        public DateTime? ExpiresAt { get; set; }
    }
}
