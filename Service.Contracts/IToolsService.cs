using Entities.Models;
using Shared.DTOs;
using System.Text;

namespace Service.Contracts;

public interface IToolsService
{
    Task<Stream> GetHttpResponseBody(string path, string accessToken);
    Task<GoogleToken> GetUserTokenAsync(bool trackChanges);
}
