

using Contracts;
using Service.Contracts;

namespace Service;

internal sealed class GmailLabelService : IGmailLabelService
{
    private readonly IRepositoryManager _repositoryManager;
    public GmailLabelService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }
}
