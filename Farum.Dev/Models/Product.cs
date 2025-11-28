namespace LittleBugShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } = "Book";
        public int StockQuantity { get; set; } = 0;
        public int LowStockThreshold { get; set; } = 5;

        // Computed property for stock status
        public string StockStatus
        {
            get
            {
                if (StockQuantity == 0) return "OutOfStock";
                if (StockQuantity <= LowStockThreshold) return "LowStock";
                return "InStock";
            }
        }

        // Check if a quantity is available
        public bool IsAvailable(int quantity)
        {
            return StockQuantity >= quantity;
        }

        // Computed properties for reviews - these will be calculated from Database.Reviews
        public decimal AverageRating
        {
            get
            {
                var reviews = LittleBugShop.Data.Database.Reviews
                    .Where(r => r.ProductId == Id && !r.IsHidden)
                    .ToList();
                
                if (!reviews.Any()) return 0;
                return (decimal)Math.Round(reviews.Average(r => r.Rating), 1);
            }
        }

        public int ReviewCount
        {
            get
            {
                return LittleBugShop.Data.Database.Reviews
                    .Count(r => r.ProductId == Id && !r.IsHidden);
            }
        }
    }
}
