using WebSaklaso.Data;
using WebSaklaso.Entities;
using WebSaklaso.Repositories.GenericRepo;

namespace WebSaklaso.Repositories.Contracts
{
    public interface ISupplierRepository : IGenericRepoBase<Supplier, AppDbContext>
    {
    }
}
