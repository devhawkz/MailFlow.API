using Contracts;
using Service.Contracts;

namespace Service;

public sealed class ServiceManager : IServiceManager
{
    private readonly Lazy<IEmailMessageService> _emailMessageService;

    private readonly Lazy<IUserService> _userService;

    private readonly Lazy<IGmailLabelService> _gmailLabelService;
    public ServiceManager(IRepositoryManager repositoryManager)
    {
       
        _emailMessageService = new Lazy<IEmailMessageService>(() => new EmailMessageService(repositoryManager));
        _userService = new Lazy<IUserService>(() => new UserService(repositoryManager));
        _gmailLabelService = new Lazy<IGmailLabelService>(() => new GmailLabelService(repositoryManager));


    }

    public IEmailMessageService EmailMessageService => _emailMessageService.Value;

    public IUserService UserService => _userService.Value;

    public IGmailLabelService GmailLabelService => _gmailLabelService.Value;

}
