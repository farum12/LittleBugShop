namespace LittleBugShop.Models
{
    public enum AddressType
    {
        Shipping,
        Billing,
        Both
    }

    public class Address
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public AddressType AddressType { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}
