namespace Service.Contracts;

public interface IServiceManager
{
    IEmailMessageService EmailMessageService { get; }
    IUserService UserService { get; }
    IGmailLabelService GmailLabelService { get; }

    }
