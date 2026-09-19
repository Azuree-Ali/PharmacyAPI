using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.DTOs.Request;
using PharmacyAPI.DTOs.Response;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Controllers
{
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.PHARMACIST_ROLE}")]
    [Area(CD.ADMIN_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IRepository<Models.Customer> _customerRepository;
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;

        public CustomerController(
            IRepository<Models.Customer> customerRepository,
            IRepository<SalesInvoice> salesInvoiceRepository)
        {
            _customerRepository = customerRepository;
            _salesInvoiceRepository = salesInvoiceRepository;
        }

        // GET: api/Customer/get
        [HttpGet("get")]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerRepository.GetAllAsync();

            var response = customers
                .Select(c => new CustomerResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone,
                    CurrentBalance = c.CurrentBalance
                })
                .ToList();

            return Ok(new ApiResponse<List<CustomerResponse>>
            {
                IsSuccess = true,
                Message = "Customers retrieved successfully",
                Data = response
            });
        }


        // GET: api/Customer/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerRepository.GetOneAsync(
                filter: c => c.Id == id
            );

            if (customer == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Customer not found"
                });
            }

            var response = new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.Phone,
                CurrentBalance = customer.CurrentBalance
            };

            return Ok(new ApiResponse<CustomerResponse>
            {
                IsSuccess = true,
                Message = "Customer retrieved successfully",
                Data = response
            });
        }


        // POST: api/Customer/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateCustomerRequest request)
        {
            var customer = new Models.Customer
            {
                Name = request.Name,
                Phone = request.Phone,
                CurrentBalance = request.CurrentBalance
            };

            await _customerRepository.CreateAsync(customer);
            await _customerRepository.CommitAsync();

            var response = new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.Phone,
                CurrentBalance = customer.CurrentBalance
            };

            return Ok(new ApiResponse<CustomerResponse>
            {
                IsSuccess = true,
                Message = "Customer created successfully",
                Data = response
            });
        }


        // PUT: api/Customer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateCustomerRequest request)
        {
            var customer = await _customerRepository.GetOneAsync(
                filter: c => c.Id == id
            );

            if (customer == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Customer not found"
                });
            }

            customer.Name = request.Name;
            customer.Phone = request.Phone;

            _customerRepository.Update(customer);

            await _customerRepository.CommitAsync();

            var response = new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.Phone,
                CurrentBalance = customer.CurrentBalance
            };

            return Ok(new ApiResponse<CustomerResponse>
            {
                IsSuccess = true,
                Message = "Customer updated successfully",
                Data = response
            });
        }


        // DELETE: api/Customer/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerRepository.GetOneAsync(
                filter: c => c.Id == id
            );

            if (customer == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Customer not found"
                });
            }

            var invoices = await _salesInvoiceRepository.GetAllAsync(
                filter: i => i.CustomerId == id
            );

            if (invoices.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "This customer cannot be deleted because they have sales invoices."
                });
            }

            _customerRepository.Delete(customer);

            await _customerRepository.CommitAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Customer deleted successfully"
            });
        }
    }
}