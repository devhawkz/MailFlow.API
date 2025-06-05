using Contracts;
using Service.Contracts;

namespace Service;

internal sealed class EmailMessageService : IEmailMessageService
{
    private readonly IRepositoryManager _repositoryManager;
    public EmailMessageService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }
}

