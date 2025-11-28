namespace LittleBugShop.Models
{
    public class PlaceOrderRequest
    {
        public int UserId { get; set; }
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    }

    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
