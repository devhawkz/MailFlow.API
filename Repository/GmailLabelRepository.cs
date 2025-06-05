using Contracts;
using Entities.Models;

namespace Repository;

public class GmailLabelRepository : RepositoryBase<GmailLabel>, IGmailLabelRepository
{
    public GmailLabelRepository(DataContext dataContext) : base(dataContext)
    {
    }
}
