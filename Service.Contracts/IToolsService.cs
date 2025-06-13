using Entities.Models;
using Shared.DTOs;
using System.Text;

namespace Service.Contracts;

public interface IToolsService
{
    Task<string> GetHttpResponseBody(string path, string accessToken, string param = "");
    Task<GoogleToken> GetUserTokenAsync(bool trackChanges);
}
