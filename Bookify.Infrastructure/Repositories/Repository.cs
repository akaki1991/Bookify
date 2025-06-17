using Bookify.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Repositories;

internal abstract class Repository<T>(ApplicationDbContext dbContext)
    where T : Entity
{
    protected readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(T entity) => _dbContext.Set<T>().Add(entity);
}
