

namespace Contracts;

public interface IUserRepository
{
    Task<Guid> GetUserIdAsync(bool trackChanges);
}
