using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

internal sealed class GmailLabelRepository : RepositoryBase<GmailLabel>, IGmailLabelRepository
{
    public GmailLabelRepository(DataContext dataContext) : base(dataContext)
    {

    }
       public async Task<GoogleToken?> GetLatestValidTokenAsync(Guid userId)
    {
        return await DataContext.GoogleTokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.ExpiresAt)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(string labelId, Guid userId)
    {
        return await DataContext.GmailLabels
            .AnyAsync(l => l.Id == labelId && l.UserId == userId);
    }

    public async Task AddAsync(GmailLabel label)
    {
        await DataContext.GmailLabels.Create;
    }
}
