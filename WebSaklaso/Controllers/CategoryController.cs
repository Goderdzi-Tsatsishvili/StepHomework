using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using WebSaklaso.Models.Category;
using WebSaklaso.Models.Common;
using WebSaklaso.Service;
using WebSaklaso.Service.Contracts;

namespace WebSaklaso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public CategoryController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }

        /// <summary>
        /// პროდუქტები კონკრეტული კატეგორიების მიხედვით
        /// </summary>
        [HttpGet("{categoryId}/products")]
        public async Task<IActionResult> GetCategoryProducts(
            [FromRoute] Guid categoryId,
            [FromQuery] PagedRequestDto parameters)
        {
            var result = await _productService.GetAllProductsOfCategoryAsync(categoryId, parameters);

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
        /// ყველა კატეგორია
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories([FromQuery] PagedRequestDto parameters)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(parameters);

            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = categories
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// კონკრეტული კატეგორია, Id პარამეტრით
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSingleCategory(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = category
            };

            return StatusCode(response.HttpStatusCode, response);
        }

        /// <summary>
        /// ახალი კატეგორიის დამატება
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerRequestExample(typeof(CategoryForCreatingDto), typeof(CategoryForCreatingDtoExample))]
        public async Task<IActionResult> AddNewCategory([FromBody] CategoryForCreatingDto newCategoryName)
        {
            var result = await _categoryService.CreateCategoryAsync(newCategoryName);

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
        /// არსებული კატეგორიის განახლება
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        [SwaggerRequestExample(typeof(CategoryForUpdatingDto), typeof(CategoryForUpdatingDtoExample))]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryForUpdatingDto model)
        {
            var result = await _categoryService.UpdateCategoryAsync(model);
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
        /// არსებული კატეგორიის წაშლა
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
        {
            await _categoryService.DeleteCategoryAsync(id);

            var response = new CommonResponse()
            {
                Message = CommonResponseMessage.SuccessMessage,
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
            };

            return StatusCode(response.HttpStatusCode, response);
        }
    }
}
