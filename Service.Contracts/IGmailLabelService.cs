
using Shared.DTOs;

namespace Service.Contracts;

public interface IGmailLabelService
{
    Task<GmailLabelListDTO> GetLabelsFromAPI(bool trackChanges);
}
