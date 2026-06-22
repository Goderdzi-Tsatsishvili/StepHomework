using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using WebSaklaso.Models.Common;
using WebSaklaso.Models.Product;
using WebSaklaso.Service.Contracts;

namespace WebSaklaso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// ყველა პროდუქტის აღება
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] PagedRequestDto request)
        {
            var products = await _productService.GetAllProductsAsync(request);
            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = products
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// კონკრეტული პროდუქტის აღება მისი იდენტიფიკატორის მიხედვით
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSingleProduct(Guid id)
        {
            var product = await _productService.GetProductAsync(id);
            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = product
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// ახალი პროდუქტის დამატება
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerRequestExample(typeof(ProductForCreatingDto), typeof(ProductForCreatingDtoExample))]
        public async Task<IActionResult> AddNewProduct([FromBody] ProductForCreatingDto newProduct)
        {
            var result = await _productService.CreateNewProductAsync(newProduct);
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
        /// პროდუქტის განახლება
        /// </summary>
        [HttpPut("{productToUpdate}")]
        [Authorize(Roles = "Admin")]
        [SwaggerRequestExample(typeof(ProductForUpdatingDto), typeof(ProductForUpdatingDtoExample))]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductForUpdatingDto model)
        {
            var result = await _productService.UpdateProductAsync(model);
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
        /// პროდუქტის წაშლა
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var result = await _productService.DeleteProductAsync(id);
            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = result
            };
            return StatusCode(response.HttpStatusCode, response);
        }
    }
}
