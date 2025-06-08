using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

internal sealed class GmailLabelRepository : RepositoryBase<GmailLabel>, IGmailLabelRepository
{
    public GmailLabelRepository(DataContext dataContext) : base(dataContext)
    {

    }

    public IEnumerable<string> FindExistingLabelsIdsAsync(Guid userId, bool trackChanges) =>
        FindByCondition(l => l.UserId.Equals(userId), trackChanges)
        .Select(l => l.Id)
        .ToList();
            

    public void AddLabelAsync(GmailLabel label) => Create(label);

    public bool CheckIfLabelExistsAsync(Guid labelId) =>
        FindByCondition(l => l.Id.Equals(labelId), trackChanges: false)
        .Any();
}
