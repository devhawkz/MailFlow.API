using Contracts;
using Service.Contracts;

namespace Service;

internal sealed class GoogleTokenService : IGoogleTokenService
{
    private readonly IRepositoryManager _repositoryManager;
    public GoogleTokenService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }
}
