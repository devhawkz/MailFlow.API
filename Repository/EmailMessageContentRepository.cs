using Contracts;
using Entities.Models;

namespace Repository;

public class EmailMessageContentRepository : RepositoryBase<EmailMessageContent>, IEmailMessageContentRepository
{
    public EmailMessageContentRepository(DataContext dataContext) : base(dataContext)
    {
    }
}2
