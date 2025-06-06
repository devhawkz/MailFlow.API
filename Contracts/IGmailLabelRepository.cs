using Entities.Models;

namespace Contracts;

public interface IGmailLabelRepository
{
    Task<GoogleToken?> GetLatestTokenForUserAsync(Guid userId);
    Task<bool> LabelExistsAsync(string labeldId, Guid userId);
    Task AddLabelAsync(GmailLabel label);

}
