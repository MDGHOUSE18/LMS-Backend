using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Context
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
