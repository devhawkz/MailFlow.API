using Contracts;
using Entities.Models;
using System.Linq.Expressions;

namespace Repository;

public class EmailMessageRepository : RepositoryBase<EmailMessage>, IEmailMessageRepository
{
    public EmailMessageRepository(DataContext dataContext) : base(dataContext)
    {
        
    }
}
