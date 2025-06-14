namespace Service.Contracts;

public interface IGmailLabelService
{
     Task<bool> DownloadAndSyncLabelsAsync(bool trackChanges, string path);
}
