namespace Service.Contracts;

public interface IServiceManager
{
    IGoogleTokenService GoogleTokenService { get; }
    IEmailMessageService EmailMessageService { get; }
    IUserService UserService { get; }
    IGmailLabelService GmailLabelService { get; }
    IEmailMessageContentService EmailMessageContentService { get; }
}
