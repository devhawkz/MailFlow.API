using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

internal sealed class GmailLabelRepository : RepositoryBase<GmailLabel>, IGmailLabelRepository
{
    public GmailLabelRepository(DataContext dataContext) : base(dataContext)
    {

    }

    public bool LabelExistsAsync(string labelId, Guid userId, bool trackChanges) =>
        FindByCondition(l => l.Id.Equals(labelId) && l.UserId.Equals(userId), trackChanges)
            .Any();

    public void AddLabelAsync(GmailLabel label) => Create(label);
   
}
