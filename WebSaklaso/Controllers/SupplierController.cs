using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using System.Security.Claims;
using WebSaklaso.Exceptions;
using WebSaklaso.Models.Common;
using WebSaklaso.Models.Supplier;
using WebSaklaso.Service;
using WebSaklaso.Service.Contracts;

namespace WebSaklaso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public SupplierController(ISupplierService supplierService, IProductService productService)
        {
            _supplierService = supplierService;
            _productService = productService;
        }

        /// <summary>
        /// პროდუქტები ავტორიზებული Supplier - ის მიხედვით
        /// </summary>
        [HttpGet("{supplierId}/products")]
        [Authorize(Roles = "Supplier")]
        public async Task<IActionResult> GetSupplierProducts([FromQuery] PagedRequestDto parameters)
        {
            var supplierId = GetAuthenticatedUserId(User);

            var result = await _productService.GetAllProductsOfSupplierAsync(supplierId, parameters);

            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = result
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// ყველა Supplier
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSuppliers([FromQuery] PagedRequestDto request)
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync(request);
            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = suppliers
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// კონკრეტული Supplier
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSingleSupplier(Guid id)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = supplier
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// ახალი Supplier
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerRequestExample(typeof(SupplierForCreatingDto), typeof(SupplierForCreatingDtoExample))]
        public async Task<IActionResult> AddNewSupplier([FromBody] SupplierForCreatingDto newSupplier)
        {
            var result = await _supplierService.CreateSupplierAsync(newSupplier);
            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.Created),
                Result = result
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// Supplier - ის განახლება
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        [SwaggerRequestExample(typeof(SupplierForUpdatingDto), typeof(SupplierForUpdatingDtoExample))]
        public async Task<IActionResult> UpdateSupplier([FromBody] SupplierForUpdatingDto model)
        {
            var result = await _supplierService.UpdateSupplierAsync(model);
            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = result
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// Supplier - ის წაშლა
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSupplier(Guid id)
        {
            await _supplierService.DeleteSupplierAsync(id);
            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK)
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        private static Guid GetAuthenticatedUserId(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedException("User is not authorized in system");

            return Guid.Parse(userId);
        }
    }
}
