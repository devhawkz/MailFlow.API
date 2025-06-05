using Contracts;
using Service.Contracts;

namespace Service;

internal sealed class UserService : IUserService
{
    private readonly IRepositoryManager _repositoryManager;
    public UserService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }
}
{
}
