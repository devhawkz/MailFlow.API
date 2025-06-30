
using Shared.DTOs;
using Shared.Responses;

namespace Service.Contracts;

public interface IGmailLabelService
{
     Task<ApiBaseResponse> DownloadAndSyncLabelsAsync(bool trackChanges, string path);
}
