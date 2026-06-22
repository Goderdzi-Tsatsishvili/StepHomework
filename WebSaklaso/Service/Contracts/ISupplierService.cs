using WebSaklaso.Models.Common;
using WebSaklaso.Models.Supplier;

namespace WebSaklaso.Service.Contracts
{
    public interface ISupplierService
    {
        Task<PagedResponseDto<SupplierForGettingDto>> GetAllSuppliersAsync(PagedRequestDto parameters);
        Task<SupplierForGettingDto> GetSupplierByIdAsync(Guid id);
        Task<SupplierForGettingDto> CreateSupplierAsync(SupplierForCreatingDto model);
        Task<SupplierForGettingDto> UpdateSupplierAsync(SupplierForUpdatingDto model);
        Task DeleteSupplierAsync(Guid id);
    }
}
