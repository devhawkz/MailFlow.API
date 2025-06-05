using Contracts;
using Service.Contracts;

namespace Service;

public sealed class ServiceManager : IServiceManager
{
    private readonly Lazy<IGoogleTokenService> _googleTokenService;

    private readonly Lazy<IEmailMessageService> _emailMessageService;

    private readonly Lazy<IUserService> _userService;

    private readonly Lazy<IGmailLabelService> _gmailLabelService;

    private readonly Lazy<IEmailMessageContentService> _emailMessageContentService;

    public ServiceManager(IRepositoryManager repositoryManager)
    {
        _googleTokenService = new Lazy<IGoogleTokenService>(() => new GoogleTokenService(repositoryManager));
        _emailMessageService = new Lazy<IEmailMessageService>(() => new EmailMessageService(repositoryManager));
        _userService = new Lazy<IUserService>(() => new UserService(repositoryManager));
        _gmailLabelService = new Lazy<IGmailLabelService>(() => new GmailLabelService(repositoryManager));
        _emailMessageContentService = new Lazy<IEmailMessageContentService>(() => new EmailMessageContentService(repositoryManager));
    }

    public IGoogleTokenService GoogleTokenService => _googleTokenService.Value;

    public IEmailMessageService EmailMessageService => _emailMessageService.Value;

    public IUserService UserService => _userService.Value;

    public IGmailLabelService GmailLabelService => _gmailLabelService.Value;

    public IEmailMessageContentService EmailMessageContentService => _emailMessageContentService.Value;
}
