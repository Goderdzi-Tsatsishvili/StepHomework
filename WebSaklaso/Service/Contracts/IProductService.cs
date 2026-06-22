using WebSaklaso.Models.Common;
using WebSaklaso.Models.Product;

namespace WebSaklaso.Service.Contracts
{
    public interface IProductService
    {
        Task<int> CreateNewProductAsync(ProductForCreatingDto request);
        Task<PagedResponseDto<ProductListForGettingDto>> GetAllProductsAsync(PagedRequestDto parameters);
        Task<ProductForGettingDto> GetProductAsync(Guid productId);
        Task<PagedResponseDto<ProductListForGettingDto>> GetAllProductsOfSupplierAsync(
            Guid supplierId,
            PagedRequestDto parameters
        );
        Task<PagedResponseDto<ProductListForGettingDto>> GetAllProductsOfCategoryAsync(
            Guid categoryId,
            PagedRequestDto parameters
        );
        Task<int> DeleteProductAsync(Guid productId);
        Task<int> UpdateProductAsync(ProductForUpdatingDto request);
    }
}
