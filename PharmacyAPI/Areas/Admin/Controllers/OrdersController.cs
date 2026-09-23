using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.PHARMACIST_ROLE}")]
    [Area(CD.ADMIN_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _itemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;


        public OrdersController(IRepository<Order> orderRepository, UserManager<ApplicationUser> userManager, IRepository<Product> productRepository, IRepository<OrderItem> itemRepository)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
            _productRepository = productRepository;
            _itemRepository = itemRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderRepository.GetAllAsync(
                includes:
                [
                    o => o.ApplicationUser
                ]
            );

            var response = orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    IsPaid = o.IsPaid,
                    TotalAmount = o.TotalAmount,
                    Discount = o.Discount,
                    DeliveryFees = o.DeliveryFees,
                    NetAmount = o.NetAmount,
                    PaymentMethod = o.PaymentMethod,
                    DeliveryAddress = o.DeliveryAddress,
                    Notes = o.Notes,
                    ApplicationUserId = o.ApplicationUserId
                })
                .ToList();

            return Ok(new ApiResponse<List<OrderResponse>>
            {
                IsSuccess = true,
                Message = "Orders retrieved successfully",
                Data = response
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderRepository.GetOneAsync(
                filter: o => o.Id == id,
                includes:
                [
                    o => o.ApplicationUser,
            o => o.OrderItems
                ]
            );

            if (order == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order not found"
                });
            }

            var response = new OrderDetailsResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                IsPaid = order.IsPaid,
                TotalAmount = order.TotalAmount,
                Discount = order.Discount,
                DeliveryFees = order.DeliveryFees,
                NetAmount = order.NetAmount,
                PaymentMethod = order.PaymentMethod,
                DeliveryAddress = order.DeliveryAddress,
                Notes = order.Notes,
                ApplicationUserId = order.ApplicationUserId,

                OrderItems = order.OrderItems.Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };

            return Ok(new ApiResponse<OrderDetailsResponse>
            {
                IsSuccess = true,
                Message = "Order retrieved successfully",
                Data = response
            });
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateOrderRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.ApplicationUserId);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            if (request.OrderItems == null || !request.OrderItems.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order must contain at least one item"
                });
            }

            var order = new Order
            {
                OrderNumber = request.OrderNumber,
                Status = Enums.OrderStatus.Pending,
                Discount = request.Discount,
                DeliveryFees = request.DeliveryFees,
                PaymentMethod = (Enums.PaymentMethod)request.PaymentMethod,
                DeliveryAddress = request.DeliveryAddress,
                Notes = request.Notes,
                ApplicationUserId = request.ApplicationUserId
            };

            decimal totalAmount = 0;

            foreach (var item in request.OrderItems)
            {
                if (item.Quantity <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Quantity must be greater than zero"
                    });
                }

                var product = await _productRepository.GetOneAsync(
                    filter: p => p.Id == item.ProductId
                );

                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Product with ID {item.ProductId} not found"
                    });
                }

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * item.Quantity
                };

                order.OrderItems.Add(orderItem);

                totalAmount += orderItem.TotalPrice;
            }

            order.TotalAmount = totalAmount;
            order.NetAmount = order.TotalAmount - order.Discount + order.DeliveryFees;

            await _orderRepository.CreateAsync(order);
            await _orderRepository.CommitAsync();

            var response = new OrderDetailsResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                IsPaid = order.IsPaid,
                TotalAmount = order.TotalAmount,
                Discount = order.Discount,
                DeliveryFees = order.DeliveryFees,
                NetAmount = order.NetAmount,
                PaymentMethod = order.PaymentMethod,
                DeliveryAddress = order.DeliveryAddress,
                Notes = order.Notes,
                ApplicationUserId = order.ApplicationUserId,

                OrderItems = order.OrderItems.Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };

            return Ok(new ApiResponse<OrderDetailsResponse>
            {
                IsSuccess = true,
                Message = "Order created successfully",
                Data = response
            });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateOrderRequest request)
        {
            var order = await _orderRepository.GetOneAsync(
                filter: o => o.Id == id,
                includes:
                [
                    o => o.OrderItems
                ]
            );

            if (order == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order not found"
                });
            }

            if (order.PaymentMethod == Enums.PaymentMethod.Cash
                && !order.IsPaid
                && request.Status is Enums.OrderStatus.Completed or Enums.OrderStatus.Cancelled)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Unpaid cash orders must be completed or cancelled through the customer delivery confirmation flow."
                });
            }

            var user = await _userManager.FindByIdAsync(request.ApplicationUserId);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            // Update Order Items
            if (request.OrderItems != null)
            {
                foreach (var item in request.OrderItems)
                {
                    if (item.ProductId <= 0)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            IsSuccess = false,
                            Message = "Invalid ProductId"
                        });
                    }

                    if (item.Quantity <= 0)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            IsSuccess = false,
                            Message = "Quantity must be greater than zero"
                        });
                    }

                    var product = await _productRepository.GetOneAsync(
                        filter: p => p.Id == item.ProductId
                    );

                    if (product == null)
                    {
                        return NotFound(new ApiResponse<object>
                        {
                            IsSuccess = false,
                            Message = $"Product with ID {item.ProductId} not found"
                        });
                    }
                }

                // Delete removed items
                var postedItemIds = request.OrderItems
                    .Where(i => i.Id > 0)
                    .Select(i => i.Id)
                    .ToList();

                var itemsToDelete = order.OrderItems
                    .Where(i => !postedItemIds.Contains(i.Id))
                    .ToList();

                foreach (var item in itemsToDelete)
                {
                    _itemRepository.Delete(item);
                }

                // Update existing items and add new ones
                foreach (var item in request.OrderItems)
                {
                    var existingItem = order.OrderItems
                        .FirstOrDefault(i => i.Id == item.Id);

                    var product = await _productRepository.GetOneAsync(
                        filter: p => p.Id == item.ProductId
                    );

                    if (existingItem != null)
                    {
                        existingItem.ProductId = product!.Id;
                        existingItem.Quantity = item.Quantity;
                        existingItem.UnitPrice = product.Price;
                        existingItem.TotalPrice = product.Price * item.Quantity;
                    }
                    else
                    {
                        var newItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = product!.Id,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price,
                            TotalPrice = product.Price * item.Quantity
                        };

                        await _itemRepository.CreateAsync(newItem);
                    }
                }
            }

            // Update Order information
            order.ApplicationUserId = request.ApplicationUserId;
            order.Status = request.Status;
            order.Discount = request.Discount;
            order.DeliveryFees = request.DeliveryFees;
            order.PaymentMethod = (Enums.PaymentMethod)request.PaymentMethod;
            order.DeliveryAddress = request.DeliveryAddress;
            // Get updated items to calculate totals
            var updatedOrder = await _orderRepository.GetOneAsync(
                filter: o => o.Id == id,
                includes:
                [
                    o => o.OrderItems
                ]
            );

            var totalAmount = updatedOrder!.OrderItems
                .Sum(i => i.Quantity * i.UnitPrice);

            order.TotalAmount = totalAmount;
            order.NetAmount = order.TotalAmount
                              - order.Discount
                              + order.DeliveryFees;

            await _orderRepository.CommitAsync();

            var response = new OrderDetailsResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                IsPaid = order.IsPaid,
                TotalAmount = order.TotalAmount,
                Discount = order.Discount,
                DeliveryFees = order.DeliveryFees,
                NetAmount = order.NetAmount,
                PaymentMethod = order.PaymentMethod,
                DeliveryAddress = order.DeliveryAddress,
                Notes = order.Notes,
                ApplicationUserId = order.ApplicationUserId,

                OrderItems = updatedOrder.OrderItems
                    .Select(item => new OrderItemResponse
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    })
                    .ToList()
            };

            return Ok(new ApiResponse<OrderDetailsResponse>
            {
                IsSuccess = true,
                Message = "Order updated successfully",
                Data = response
            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderRepository.GetOneAsync(
                filter: o => o.Id == id,
                includes:
                [
                    o => o.OrderItems,
            o => o.Notifications
                ]
            );

            if (order == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Order not found"
                });
            }

            if (order.PaymentMethod == Enums.PaymentMethod.Cash
                && order.Status == Enums.OrderStatus.Pending
                && !order.IsPaid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Cancel this cash order through the customer delivery flow before deleting it so reserved inventory can be restored."
                });
            }
            // Delete order items
            foreach (var item in order.OrderItems.ToList())
            {
                _itemRepository.Delete(item);
            }

            // Delete order
            _orderRepository.Delete(order);

            await _orderRepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Order deleted successfully"
            });
        }
    }
}
