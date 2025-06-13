using Entities.Models;

namespace Contracts;

public interface IGmailLabelRepository
{
    Task<IEnumerable<string>> FindExistingLabelsIdsAsync(Guid userId, bool trackChanges);
    Task<bool> CheckIfLabelExistsAsync(Guid labelId);
    void AddRangeLabels(IEnumerable<GmailLabel> labels);

}
