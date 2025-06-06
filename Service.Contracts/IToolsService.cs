using Entities.Models;
using Shared.DTOs;
using System.Text;

namespace Service.Contracts;

public interface IToolsService
{
    Task<string> GetHttpResponseBody(string path, string accessToken);
    void AddLabelsToDb(GmailLabelListDTO labelList, Guid userId, bool trackChanges);
    Task<GoogleToken> GetUserTokenAsync();
}
