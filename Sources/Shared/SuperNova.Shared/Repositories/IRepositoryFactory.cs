
using SuperNova.Shared.DomainObjects;
using System.Threading.Tasks;

namespace SuperNova.Shared.Repositories
{
    public interface IRepositoryFactory
    {
        Task<TDocumentStore> CreateDocumentRepositoryAsync<TDocumentStore>(Tenant tenant) where TDocumentStore : IRepository;
        Task<TTableStore> CreateTableRepositoryAsync<TTableStore>(Tenant tenant) where TTableStore : IRepository;
    }
}
