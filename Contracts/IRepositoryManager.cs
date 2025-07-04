﻿namespace Contracts;

public interface IRepositoryManager
{
    IGoogleTokenRepository GoogleToken { get; }
    IEmailMessageRepository EmailMessage { get; }
    IUserRepository User { get; }
    IGmailLabelRepository GmailLabel { get; }
    IEmailMessageContentRepository EmailMessageContent { get; }
    IToolsRepository Tools { get; }
    Task SaveAsync();
}

