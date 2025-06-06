using Entities.Models;

namespace Contracts;

public interface IGmailLabelRepository
{
    bool LabelExistsAsync(string labelId, Guid userId, bool trackChanges);
    void AddLabelAsync(GmailLabel label);

}
