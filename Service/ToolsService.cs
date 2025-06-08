using Contracts;
using Entities.Models;
using Newtonsoft.Json.Linq;
using Service.Contracts;
using Shared.DTOs;
using System.Net.Http.Headers;

namespace Service;

public sealed class ToolsService : IToolsService
{
    private readonly IRepositoryManager _repositoryManager;
    public ToolsService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }
    public async Task<string> GetHttpResponseBody(string path, string accessToken, string param = "")
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync(path);
        if (!response.IsSuccessStatusCode)
            return string.Empty;

        var content = await response.Content.ReadAsStringAsync();
        return content;
    }

   
    public async Task<GoogleToken> GetUserTokenAsync()
    {
        var token = _repositoryManager.GoogleToken.GetLatestTokenForUserAsync(trackChanges: false);
        if (token is null)
            return null!;

        if (token.ExpiresAt < DateTime.UtcNow)
        {
            var credential = await _repositoryManager.Tools.GetUserCredentialAsync();
            await credential.RefreshTokenAsync(CancellationToken.None);

            token = _repositoryManager.Tools.RefreshToken(credential, token);
            _repositoryManager.GoogleToken.UpdateTokenAsync(token);
        }

        return token;
    }
}
