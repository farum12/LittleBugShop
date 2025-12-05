using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LittleBugShop.Data;
using LittleBugShop.Models;

namespace LittleBugShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetProducts(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? genre = null,
            [FromQuery] string? author = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string sortBy = "name", 
            [FromQuery] string sortOrder = "asc")
        {
            IEnumerable<Product> products = Database.Products;

            // Apply search filter (searches in name, author, description)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                products = products.Where(p => 
                    p.Name.ToLower().Contains(search) ||
                    p.Author.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search));
            }

            // Filter by genre
            if (!string.IsNullOrWhiteSpace(genre))
            {
                products = products.Where(p => p.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by author
            if (!string.IsNullOrWhiteSpace(author))
            {
                products = products.Where(p => p.Author.Equals(author, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // Apply sorting
            switch (sortBy?.ToLower())
            {
                case "name":
                    products = sortOrder.ToLower() == "desc" ? products.OrderByDescending(p => p.Name) : products.OrderBy(p => p.Name);
                    break;
                case "author":
                    products = sortOrder.ToLower() == "desc" ? products.OrderByDescending(p => p.Author) : products.OrderBy(p => p.Author);
                    break;
                case "price":
                    products = sortOrder.ToLower() == "desc" ? products.OrderByDescending(p => p.Price) : products.OrderBy(p => p.Price);
                    break;
                case "genre":
                    products = sortOrder.ToLower() == "desc" ? products.OrderByDescending(p => p.Genre) : products.OrderBy(p => p.Genre);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            return Ok(products);
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProduct(int id)
        {
            var product = Database.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult<Product> CreateProduct(Product product)
        {
            product.Id = Database.Products.Max(p => p.Id) + 1;
            Database.Products.Add(product);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, Product updatedProduct)
        {
            var product = Database.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Description = updatedProduct.Description;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = Database.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            Database.Products.Remove(product);
            return NoContent();
        }

        [HttpGet("{id}/availability")]
        public ActionResult<object> CheckAvailability(int id, [FromQuery] int quantity = 1)
        {
            var product = Database.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                productId = product.Id,
                productName = product.Name,
                stockQuantity = product.StockQuantity,
                stockStatus = product.StockStatus,
                requestedQuantity = quantity,
                isAvailable = product.IsAvailable(quantity)
            });
        }

        [HttpPut("{id}/stock")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateStock(int id, [FromBody] StockUpdateRequest request)
        {
            var product = Database.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            if (request.Quantity < 0)
            {
                return BadRequest("Stock quantity cannot be negative.");
            }

            product.StockQuantity = request.Quantity;
            return Ok(new
            {
                productId = product.Id,
                productName = product.Name,
                stockQuantity = product.StockQuantity,
                stockStatus = product.StockStatus
            });
        }

        [HttpPost("{id}/stock/increase")]
        [Authorize(Roles = "Admin")]
        public IActionResult IncreaseStock(int id, [FromBody] StockChangeRequest request)
        {
            var product = Database.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than zero.");
            }

            product.StockQuantity += request.Amount;
            return Ok(new
            {
                productId = product.Id,
                productName = product.Name,
                stockQuantity = product.StockQuantity,
                stockStatus = product.StockStatus,
                message = $"Stock increased by {request.Amount}"
            });
        }

        [HttpPost("{id}/stock/decrease")]
        [Authorize(Roles = "Admin")]
        public IActionResult DecreaseStock(int id, [FromBody] StockChangeRequest request)
        {
            var product = Database.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than zero.");
            }

            if (product.StockQuantity < request.Amount)
            {
                return BadRequest($"Cannot decrease stock by {request.Amount}. Current stock: {product.StockQuantity}");
            }

            product.StockQuantity -= request.Amount;
            return Ok(new
            {
                productId = product.Id,
                productName = product.Name,
                stockQuantity = product.StockQuantity,
                stockStatus = product.StockStatus,
                message = $"Stock decreased by {request.Amount}"
            });
        }
    }

    public class StockUpdateRequest
    {
        public int Quantity { get; set; }
    }

    public class StockChangeRequest
    {
        public int Amount { get; set; }
    }
}
