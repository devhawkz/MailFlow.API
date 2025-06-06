using Contracts;

namespace Repository;

public sealed class RepositoryManager : IRepositoryManager
{
    private readonly DataContext _dataContext;
    private readonly IConfiguration _config;
    private readonly Lazy<IGoogleTokenRepository> _googleTokenRepository;

    private readonly Lazy<IEmailMessageRepository> _emailMessageRepository;

    private readonly Lazy<IUserRepository> _userRepository;

    private readonly Lazy<IGmailLabelRepository> _gmailLabelRepository;

    private readonly Lazy<IEmailMessageContentRepository> _emailMessageContentRepository;
    private readonly Lazy<IToolsRepository> _toolsRepository;

    public RepositoryManager(DataContext dataContext)
    {
        _dataContext = dataContext;
        _googleTokenRepository = new Lazy<IGoogleTokenRepository>(() => new GoogleTokenRepository(dataContext));
        _emailMessageRepository = new Lazy<IEmailMessageRepository>(() => new EmailMessageRepository(dataContext));
        _userRepository = new Lazy<IUserRepository>(() => new UserRepository(dataContext));
        _gmailLabelRepository = new Lazy<IGmailLabelRepository>(() => new GmailLabelRepository(dataContext));
        _emailMessageContentRepository = new Lazy<IEmailMessageContentRepository>(() => new EmailMessageContentRepository(dataContext));
        _toolsRepository = new Lazy<IToolsRepository>(() => new ToolsRepository(_config));
    }

    public IGoogleTokenRepository GoogleToken => _googleTokenRepository.Value;

    public IEmailMessageRepository EmailMessage => _emailMessageRepository.Value;

    public IUserRepository User => _userRepository.Value;

    public IGmailLabelRepository GmailLabel => _gmailLabelRepository.Value;

    public IEmailMessageContentRepository EmailMessageContent => _emailMessageContentRepository.Value;

    public IToolsRepository Tools => _toolsRepository.Value;

    public void Save() => _dataContext.SaveChanges();

}