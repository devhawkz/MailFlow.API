using Contracts;
using Service.Contracts;

namespace Service;

internal sealed class EmailMessageContentService : IEmailMessageContentService
{
    private readonly IRepositoryManager _repositoryManager;
    public EmailMessageContentService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }
}
{
}
