using Entities.Models;

namespace Contracts;

public interface IGmailLabelRepository
{
    IEnumerable<string> FindExistingLabelsIdsAsync(Guid userId, bool trackChanges);
    bool CheckIfLabelExistsAsync(Guid labelId);
    void AddLabelAsync(GmailLabel label);

}
