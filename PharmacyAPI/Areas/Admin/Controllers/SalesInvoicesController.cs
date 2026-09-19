using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class SalesInvoicesController : ControllerBase
    {
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly IRepository<Models.Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ProductBatch> _batchRepository;
        private readonly IRepository<SalesInvoiceItem> _itemRepository;
        public SalesInvoicesController(IRepository<SalesInvoice> salesInvoiceRepository, IRepository<Models.Customer> customerRepository, IRepository<Order> orderRepository, IRepository<ProductBatch> batchRepository, IRepository<SalesInvoiceItem> itemRepository)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _batchRepository = batchRepository;
            _itemRepository = itemRepository;
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _salesInvoiceRepository.GetAllAsync(
                includes:
                [
                    i => i.Customer,
            i => i.Order
                ]
            );

            var response = invoices
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i => new SalesInvoiceResponse
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    InvoiceDate = i.InvoiceDate,
                    TotalAmount = i.TotalAmount,
                    Discount = i.Discount,
                    NetAmount = i.NetAmount,
                    CustomerId = i.CustomerId,
                    OrderId = i.OrderId
                })
                .ToList();

            return Ok(new ApiResponse<List<SalesInvoiceResponse>>
            {
                IsSuccess = true,
                Message = "Invoices retrieved successfully",
                Data = response
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _salesInvoiceRepository.GetOneAsync(
                filter: i => i.Id == id,
                includes:
                [
                    i => i.Customer,
            i => i.Order,
            i => i.InvoiceItems
                ]
            );

            if (invoice == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invoice not found"
                });
            }

            var response = new SalesInvoiceDetailsResponse
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                TotalAmount = invoice.TotalAmount,
                Discount = invoice.Discount,
                NetAmount = invoice.NetAmount,
                CustomerId = invoice.CustomerId,
                OrderId = invoice.OrderId,

                InvoiceItems = invoice.InvoiceItems
                    .Select(item => new SalesInvoiceItemResponse
                    {
                        Id = item.Id,
                        ProductBatchId = item.ProductBatchId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    })
                    .ToList()
            };

            return Ok(new ApiResponse<SalesInvoiceDetailsResponse>
            {
                IsSuccess = true,
                Message = "Invoice retrieved successfully",
                Data = response
            });
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateSalesInvoiceRequest request)
        {
            if (request.CustomerId == null && request.OrderId == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invoice must belong to a customer or an online order."
                });
            }

            if (request.CustomerId != null && request.OrderId != null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invoice cannot belong to both a customer and an order."
                });
            }

            if (request.InvoiceItems == null || !request.InvoiceItems.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invoice must contain at least one item."
                });
            }

            Models.Customer? customer = null;

            // Customer
            if (request.CustomerId.HasValue)
            {
                customer = await _customerRepository.GetOneAsync(
                    filter: c => c.Id == request.CustomerId.Value
                );

                if (customer == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Customer not found."
                    });
                }
            }

            // Order
            if (request.OrderId.HasValue)
            {
                var order = await _orderRepository.GetOneAsync(
                    filter: o => o.Id == request.OrderId.Value
                );

                if (order == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Order not found."
                    });
                }
            }

            var invoice = new SalesInvoice
            {
                InvoiceNumber = request.InvoiceNumber,
                InvoiceDate = DateTime.Now,
                CustomerId = request.CustomerId,
                OrderId = request.OrderId,
                Discount = request.Discount
            };

            decimal total = 0;

            foreach (var itemRequest in request.InvoiceItems)
            {
                if (itemRequest.ProductBatchId <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Please select a valid product batch."
                    });
                }

                if (itemRequest.Quantity <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Quantity must be greater than zero."
                    });
                }

                var batch = await _batchRepository.GetOneAsync(
                    filter: b => b.Id == itemRequest.ProductBatchId,
                    includes:
                    [
                        b => b.Product
                    ]
                );

                if (batch == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Batch #{itemRequest.ProductBatchId} was not found."
                    });
                }

                if (batch.Product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Product for the selected batch was not found."
                    });
                }

                if (itemRequest.Quantity > batch.QuantityOnHand)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = $"Not enough stock for batch {batch.BatchNumber}."
                    });
                }

                // Price comes from database
                decimal unitPrice = batch.Product.Price;

                var invoiceItem = new SalesInvoiceItem
                {
                    ProductBatchId = batch.Id,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * itemRequest.Quantity
                };

                total += invoiceItem.TotalPrice;

                // Decrease stock
                batch.QuantityOnHand -= itemRequest.Quantity;

                _batchRepository.Update(batch);

                invoice.InvoiceItems.Add(invoiceItem);
            }

            invoice.TotalAmount = total;

            invoice.NetAmount =
                invoice.TotalAmount - invoice.Discount;

            if (invoice.NetAmount < 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Discount cannot be greater than total amount."
                });
            }

            // Customer Balance
            if (customer != null)
            {
                customer.CurrentBalance += invoice.NetAmount;

                _customerRepository.Update(customer);
            }

            await _salesInvoiceRepository.CreateAsync(invoice);
            await _salesInvoiceRepository.CommitAsync();

            var response = new SalesInvoiceDetailsResponse
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                TotalAmount = invoice.TotalAmount,
                Discount = invoice.Discount,
                NetAmount = invoice.NetAmount,
                CustomerId = invoice.CustomerId,
                OrderId = invoice.OrderId,

                InvoiceItems = invoice.InvoiceItems
                    .Select(item => new SalesInvoiceItemResponse
                    {
                        Id = item.Id,
                        ProductBatchId = item.ProductBatchId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    })
                    .ToList()
            };

            return Ok(new ApiResponse<SalesInvoiceDetailsResponse>
            {
                IsSuccess = true,
                Message = "Invoice created successfully",
                Data = response
            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _salesInvoiceRepository.GetOneAsync(
                filter: i => i.Id == id,
                includes:
                [
                    i => i.InvoiceItems,
            i => i.Customer
                ]
            );

            if (invoice == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invoice not found."
                });
            }

            // Return Customer Balance
            if (invoice.Customer != null)
            {
                invoice.Customer.CurrentBalance -= invoice.NetAmount;

                if (invoice.Customer.CurrentBalance < 0)
                {
                    invoice.Customer.CurrentBalance = 0;
                }

                _customerRepository.Update(invoice.Customer);
            }

            // Return Stock
            foreach (var item in invoice.InvoiceItems.ToList())
            {
                var batch = await _batchRepository.GetOneAsync(
                    filter: b => b.Id == item.ProductBatchId
                );

                if (batch != null)
                {
                    batch.QuantityOnHand += item.Quantity;

                    _batchRepository.Update(batch);
                }

                _itemRepository.Delete(item);
            }

            // Delete Invoice
            _salesInvoiceRepository.Delete(invoice);

            await _salesInvoiceRepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Invoice deleted successfully"
            });
        }
    }
}
