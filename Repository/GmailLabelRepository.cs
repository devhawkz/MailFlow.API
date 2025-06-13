using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

internal sealed class GmailLabelRepository : RepositoryBase<GmailLabel>, IGmailLabelRepository
{
    public GmailLabelRepository(DataContext dataContext) : base(dataContext)
    {

    }

    public async Task<IEnumerable<string>> FindExistingLabelsIdsAsync(Guid userId, bool trackChanges) => await
        FindByCondition(l => l.UserId.Equals(userId), trackChanges)
        .Select(l => l.Id)
        .ToListAsync();
            

    public void AddRangeLabels(IEnumerable<GmailLabel> labels) => CreateRange(labels);

    public async Task<bool> CheckIfLabelExistsAsync(Guid labelId) => await
        FindByCondition(l => l.Id.Equals(labelId), trackChanges: false)
        .AnyAsync();
}
