
using Shared.DTOs;
using Shared.Responses;

namespace Service.Contracts;

public interface IGmailLabelService
{
     Task<ApiResponse<GmailLabelListDTO>> DownloadAndSyncLabelsAsync(bool trackChanges, string path);
}
