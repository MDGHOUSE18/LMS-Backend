using System.Threading.Tasks;
using System.Threading;

namespace LMS.Infrastructure.Persistence.Context
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
