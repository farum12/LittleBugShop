using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LittleBugShop.Data;
using LittleBugShop.Models;

namespace LittleBugShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        /// <summary>
        /// Get current user's wishlist with full product details
        /// </summary>
        [HttpGet]
        public IActionResult GetWishlist()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var wishlist = Database.Wishlists.FirstOrDefault(w => w.UserId == userId);

            if (wishlist == null)
            {
                return Ok(new
                {
                    userId,
                    items = new List<object>(),
                    totalItems = 0
                });
            }

            var items = wishlist.ProductIds
                .Select(productId => Database.Products.FirstOrDefault(p => p.Id == productId))
                .Where(product => product != null)
                .Select(product => new
                {
                    product!.Id,
                    product.Name,
                    product.Author,
                    product.Genre,
                    product.Price,
                    product.StockQuantity,
                    product.AverageRating,
                    product.ReviewCount,
                    InStock = product.StockQuantity > 0
                })
                .ToList();

            return Ok(new
            {
                userId,
                items,
                totalItems = items.Count
            });
        }

        /// <summary>
        /// Add product to wishlist
        /// </summary>
        [HttpPost("items/{productId}")]
        public IActionResult AddToWishlist(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var product = Database.Products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            var wishlist = Database.Wishlists.FirstOrDefault(w => w.UserId == userId);

            if (wishlist == null)
            {
                // Create new wishlist for user
                wishlist = new Wishlist
                {
                    Id = Database.Wishlists.Any() ? Database.Wishlists.Max(w => w.Id) + 1 : 1,
                    UserId = userId,
                    ProductIds = new List<int>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                Database.Wishlists.Add(wishlist);
            }

            // Check if product already in wishlist
            if (wishlist.ProductIds.Contains(productId))
                return BadRequest(new { message = "Product already in wishlist" });

            wishlist.ProductIds.Add(productId);
            wishlist.UpdatedAt = DateTime.UtcNow;

            return Ok(new
            {
                message = "Product added to wishlist",
                product = new
                {
                    product.Id,
                    product.Name,
                    product.Author,
                    product.Price
                },
                totalItems = wishlist.ProductIds.Count
            });
        }

        /// <summary>
        /// Remove product from wishlist
        /// </summary>
        [HttpDelete("items/{productId}")]
        public IActionResult RemoveFromWishlist(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var wishlist = Database.Wishlists.FirstOrDefault(w => w.UserId == userId);

            if (wishlist == null || !wishlist.ProductIds.Contains(productId))
                return NotFound(new { message = "Product not in wishlist" });

            wishlist.ProductIds.Remove(productId);
            wishlist.UpdatedAt = DateTime.UtcNow;

            return Ok(new
            {
                message = "Product removed from wishlist",
                totalItems = wishlist.ProductIds.Count
            });
        }

        /// <summary>
        /// Clear entire wishlist
        /// </summary>
        [HttpDelete]
        public IActionResult ClearWishlist()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var wishlist = Database.Wishlists.FirstOrDefault(w => w.UserId == userId);

            if (wishlist == null)
                return Ok(new { message = "Wishlist already empty" });

            wishlist.ProductIds.Clear();
            wishlist.UpdatedAt = DateTime.UtcNow;

            return Ok(new { message = "Wishlist cleared" });
        }

        /// <summary>
        /// Check if product is in wishlist
        /// </summary>
        [HttpGet("check/{productId}")]
        public IActionResult CheckInWishlist(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var wishlist = Database.Wishlists.FirstOrDefault(w => w.UserId == userId);

            var inWishlist = wishlist != null && wishlist.ProductIds.Contains(productId);

            return Ok(new
            {
                productId,
                inWishlist
            });
        }

        /// <summary>
        /// Move all wishlist items to cart
        /// </summary>
        [HttpPost("move-to-cart")]
        public IActionResult MoveToCart()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var wishlist = Database.Wishlists.FirstOrDefault(w => w.UserId == userId);

            if (wishlist == null || !wishlist.ProductIds.Any())
                return BadRequest(new { message = "Wishlist is empty" });

            var cart = Database.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Database.Carts.Any() ? Database.Carts.Max(c => c.Id) + 1 : 1,
                    UserId = userId,
                    Items = new List<CartItem>()
                };
                Database.Carts.Add(cart);
            }

            var addedCount = 0;
            var skippedCount = 0;
            var outOfStockProducts = new List<string>();

            foreach (var productId in wishlist.ProductIds.ToList())
            {
                var product = Database.Products.FirstOrDefault(p => p.Id == productId);
                if (product == null) continue;

                // Check if product is in stock
                if (product.StockQuantity <= 0)
                {
                    outOfStockProducts.Add(product.Name);
                    skippedCount++;
                    continue;
                }

                // Check if already in cart
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (existingItem != null)
                {
                    // Increase quantity if in stock
                    if (existingItem.Quantity < product.StockQuantity)
                    {
                        existingItem.Quantity++;
                        addedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                else
                {
                    // Add new item to cart
                    cart.Items.Add(new CartItem
                    {
                        Id = cart.Items.Any() ? cart.Items.Max(i => i.Id) + 1 : 1,
                        ProductId = productId,
                        ProductName = product.Name,
                        Author = product.Author,
                        UnitPrice = product.Price,
                        Quantity = 1
                    });
                    addedCount++;
                }
            }

            // Clear wishlist after moving items
            wishlist.ProductIds.Clear();
            wishlist.UpdatedAt = DateTime.UtcNow;

            return Ok(new
            {
                message = "Items moved to cart",
                addedToCart = addedCount,
                skipped = skippedCount,
                outOfStockProducts = outOfStockProducts,
                cartTotalItems = cart.Items.Sum(i => i.Quantity)
            });
        }

        /// <summary>
        /// Get wishlist item count
        /// </summary>
        [HttpGet("count")]
        public IActionResult GetWishlistCount()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var wishlist = Database.Wishlists.FirstOrDefault(w => w.UserId == userId);

            return Ok(new
            {
                count = wishlist?.ProductIds.Count ?? 0
            });
        }
    }
}
