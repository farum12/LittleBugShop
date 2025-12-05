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
                return NotFound(new ErrorResponse(404, $"Product with ID {id} not found."));
            }
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult<Product> CreateProduct(Product product)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                return BadRequest(new ErrorResponse(400, "Product name is required."));
            }

            if (product.Name.Length < 2)
            {
                return BadRequest(new ErrorResponse(400, "Product name must be at least 2 characters long."));
            }

            if (product.Name.Length > 200)
            {
                return BadRequest(new ErrorResponse(400, "Product name cannot exceed 200 characters."));
            }

            if (string.IsNullOrWhiteSpace(product.Author))
            {
                return BadRequest(new ErrorResponse(400, "Author is required."));
            }

            if (string.IsNullOrWhiteSpace(product.Genre))
            {
                return BadRequest(new ErrorResponse(400, "Genre is required."));
            }

            if (string.IsNullOrWhiteSpace(product.Description))
            {
                return BadRequest(new ErrorResponse(400, "Description is required."));
            }

            // Validate price
            if (product.Price <= 0)
            {
                return BadRequest(new ErrorResponse(400, "Price must be greater than zero."));
            }

            if (product.Price > 999999.99m)
            {
                return BadRequest(new ErrorResponse(400, "Price cannot exceed 999,999.99."));
            }

            // Validate stock quantity
            if (product.StockQuantity < 0)
            {
                return BadRequest(new ErrorResponse(400, "Stock quantity cannot be negative."));
            }

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
                return NotFound(new ErrorResponse(404, $"Product with ID {id} not found."));
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
                return NotFound(new ErrorResponse(404, $"Product with ID {id} not found."));
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
                return NotFound(new ErrorResponse(404, $"Product with ID {id} not found."));
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
                return NotFound(new ErrorResponse(404, $"Product with ID {id} not found."));
            }

            if (request.Quantity < 0)
            {
                return BadRequest(new ErrorResponse(400, "Stock quantity cannot be negative."));
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
                return NotFound(new ErrorResponse(404, $"Product with ID {id} not found."));
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new ErrorResponse(400, "Amount must be greater than zero."));
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
                return NotFound(new ErrorResponse(404, $"Product with ID {id} not found."));
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new ErrorResponse(400, "Amount must be greater than zero."));
            }

            if (product.StockQuantity < request.Amount)
            {
                return BadRequest(new ErrorResponse(400, $"Cannot decrease stock by {request.Amount}. Current stock: {product.StockQuantity}"));
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
