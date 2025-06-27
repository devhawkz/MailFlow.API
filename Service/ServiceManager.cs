using Contracts;
using Microsoft.Extensions.Configuration;
using Service.Contracts;

namespace Service;

public sealed class ServiceManager : IServiceManager
{
    private readonly Lazy<IEmailMessageService> _emailMessageService;

    private readonly Lazy<IUserService> _userService;

    private readonly Lazy<IGmailLabelService> _gmailLabelService;

    private readonly IToolsService _toolsService;
    private readonly IConfiguration _configuration;
    private readonly ILoggerManager _logger;
    public ServiceManager(IRepositoryManager repositoryManager, IToolsService toolsService, IConfiguration configuration, ILoggerManager logger)
    {
        _toolsService = toolsService;
        _configuration = configuration;
        _logger = logger;

        _emailMessageService = new Lazy<IEmailMessageService>(() => new EmailMessageService(repositoryManager));
        _userService = new Lazy<IUserService>(() => new UserService(repositoryManager, _configuration));
        _gmailLabelService = new Lazy<IGmailLabelService>(() => new GmailLabelService(repositoryManager, _toolsService, _logger));


    }

    public IEmailMessageService EmailMessageService => _emailMessageService.Value;

    public IUserService UserService => _userService.Value;

    public IGmailLabelService GmailLabelService => _gmailLabelService.Value;

}
