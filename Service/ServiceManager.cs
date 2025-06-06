using Contracts;
using Service.Contracts;

namespace Service;

public sealed class ServiceManager : IServiceManager
{
    private readonly Lazy<IEmailMessageService> _emailMessageService;

    private readonly Lazy<IUserService> _userService;

    private readonly Lazy<IGmailLabelService> _gmailLabelService;

    private readonly IToolsService _toolsService;
    public ServiceManager(IRepositoryManager repositoryManager, IToolsService toolsService)
    {
       
        _emailMessageService = new Lazy<IEmailMessageService>(() => new EmailMessageService(repositoryManager));
        _userService = new Lazy<IUserService>(() => new UserService(repositoryManager));
        _toolsService = toolsService;
        _gmailLabelService = new Lazy<IGmailLabelService>(() => new GmailLabelService(repositoryManager,_toolsService));


    }

    public IEmailMessageService EmailMessageService => _emailMessageService.Value;

    public IUserService UserService => _userService.Value;

    public IGmailLabelService GmailLabelService => _gmailLabelService.Value;

}
