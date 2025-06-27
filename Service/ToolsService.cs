using Contracts;
using Entities.Models;
using Newtonsoft.Json.Linq;
using Service.Contracts;
using Shared.DTOs;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Service;

public sealed class ToolsService : IToolsService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IGmailApiClient _httpClient;

    public ToolsService(IRepositoryManager repositoryManager, IGmailApiClient httpClient)
    {
        _repositoryManager = repositoryManager;
        _httpClient = httpClient;
    }
    public async Task<string?> GetHttpResponseBody(string path, string accessToken) => await _httpClient.GetAsync(path, accessToken);
   
    public async Task<GoogleToken> GetUserTokenAsync(bool trackChanges)
    {
        var token = await _repositoryManager.GoogleToken.GetLatestTokenForUserAsync(trackChanges);
        if (token is null)
            return null!; // problem

        if (token.ExpiresAt < DateTime.UtcNow)
        {
            var credential = await _repositoryManager.Tools.GetUserCredentialAsync();
            await credential.RefreshTokenAsync(CancellationToken.None);

            token = _repositoryManager.Tools.RefreshToken(credential, token);
            _repositoryManager.GoogleToken.UpdateToken(token);
        }

        return token;
    }
}
