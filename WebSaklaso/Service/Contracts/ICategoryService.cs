using WebSaklaso.Models.Category;
using WebSaklaso.Models.Common;

namespace WebSaklaso.Service.Contracts
{
    public interface ICategoryService
    {
        Task<PagedResponseDto<CategoryForGettingDto>> GetAllCategoriesAsync(PagedRequestDto parameters);
        Task<CategoryForGettingDto> GetCategoryByIdAsync(Guid id);
        Task<Guid> CreateCategoryAsync(CategoryForCreatingDto model);
        Task<CategoryForGettingDto> UpdateCategoryAsync(CategoryForUpdatingDto model);
        Task DeleteCategoryAsync(Guid id);
    }
}
