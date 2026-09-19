using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.DTOs.Request;
using PharmacyAPI.DTOs.Response;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Customer.Controllers
{
    [Authorize]
    [Area(CD.CUSTOMER_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            IRepository<Cart> cartRepository,
            IRepository<CartItem> cartItemRepository,
            IRepository<Product> productRepository,
            UserManager<ApplicationUser> userManager)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        // GET: api/Customer/Cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var cart = await _cartRepository.GetOneAsync(
                filter: c => c.ApplicationUserId == userId,
                includes: [c => c.CartItems]
            );

            if (cart == null)
            {
                cart = new Cart
                {
                    ApplicationUserId = userId
                };

                await _cartRepository.CreateAsync(cart);
                await _cartRepository.CommitAsync();
            }

            var response = new CartResponse
            {
                Id = cart.Id,
                Items = cart.CartItems.Select(item => new CartItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? string.Empty,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                }).ToList(),

                TotalAmount = cart.CartItems.Sum(item => item.TotalPrice)
            };

            return Ok(new ApiResponse<CartResponse>
            {
                IsSuccess = true,
                Message = "Cart retrieved successfully.",
                Data = response
            });
        }

        // POST: api/Customer/Cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddToCart(AddToCartRequest request)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var product = await _productRepository.GetOneAsync(
                filter: p => p.Id == request.ProductId,
                IsTracking: false
            );

            if (product == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Product not found."
                });
            }

            var cart = await _cartRepository.GetOneAsync(
                filter: c => c.ApplicationUserId == userId,
                includes: [c => c.CartItems]
            );

            if (cart == null)
            {
                cart = new Cart
                {
                    ApplicationUserId = userId
                };

                await _cartRepository.CreateAsync(cart);
                await _cartRepository.CommitAsync();
            }

            var existingItem = cart.CartItems
                .FirstOrDefault(x => x.ProductId == request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                existingItem.UnitPrice = product.Price;
                existingItem.TotalPrice =
                    existingItem.Quantity * existingItem.UnitPrice;

                _cartItemRepository.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = request.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = request.Quantity * product.Price
                };

                await _cartItemRepository.CreateAsync(cartItem);
            }

            await _cartRepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Product added to cart successfully."
            });
        }

        // PUT: api/Customer/Cart/items/{id}
        [HttpPut("items/{id:int}")]
        public async Task<IActionResult> UpdateCartItem(
            int id,
            UpdateCartItemRequest request)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var cart = await _cartRepository.GetOneAsync(
                filter: c => c.ApplicationUserId == userId,
                includes: [c => c.CartItems]
            );

            if (cart == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Cart not found."
                });
            }

            var item = cart.CartItems.FirstOrDefault(x => x.Id == id);

            if (item == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Cart item not found."
                });
            }

            var product = await _productRepository.GetOneAsync(
                filter: p => p.Id == item.ProductId,
                IsTracking: false
            );

            if (product == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Product not found."
                });
            }

            item.Quantity = request.Quantity;
            item.UnitPrice = product.Price;
            item.TotalPrice = item.Quantity * item.UnitPrice;

            _cartItemRepository.Update(item);

            await _cartRepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Cart item updated successfully."
            });
        }

        // DELETE: api/Customer/Cart/items/{id}
        [HttpDelete("items/{id:int}")]
        public async Task<IActionResult> RemoveCartItem(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var cart = await _cartRepository.GetOneAsync(
                filter: c => c.ApplicationUserId == userId,
                includes: [c => c.CartItems]
            );

            if (cart == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Cart not found."
                });
            }

            var item = cart.CartItems.FirstOrDefault(x => x.Id == id);

            if (item == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Cart item not found."
                });
            }

            _cartItemRepository.Delete(item);

            await _cartRepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Product removed from cart successfully."
            });
        }

        // DELETE: api/Customer/Cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User is not authenticated."
                });
            }

            var cart = await _cartRepository.GetOneAsync(
                filter: c => c.ApplicationUserId == userId,
                includes: [c => c.CartItems]
            );

            if (cart == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Cart not found."
                });
            }

            foreach (var item in cart.CartItems.ToList())
            {
                _cartItemRepository.Delete(item);
            }

            await _cartRepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Cart cleared successfully."
            });
        }
    }
}