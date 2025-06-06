namespace Service.Contracts;

public interface IGmailLabelService
{
     Task<bool> GetLabelsFromAPI(bool trackChanges);
}
