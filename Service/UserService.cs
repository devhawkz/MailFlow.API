using Contracts;
using Entities.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Service.Contracts;
using System.Runtime.CompilerServices;

namespace Service;

internal sealed class UserService : IUserService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IConfiguration _config;
    public UserService(IRepositoryManager repositoryManager, IConfiguration configuration)
    {
        _repositoryManager = repositoryManager;
        _config = configuration;
    }

    public async Task AuthorizeUser()
    {
        var credential = await GetUserCredentialAsync();
        
        var userId = await _repositoryManager.User.GetUserIdAsync(trackChanges: false);

        await credential.RefreshTokenAsync(CancellationToken.None);

        var expiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);

        var userToken = await _repositoryManager.GoogleToken.GetLatestTokenForUserAsync(trackChanges: false);
        
        if(userToken is not null)
        {
            userToken.AccessToken = credential.Token.AccessToken;
            userToken.RefreshToken = credential.Token.RefreshToken;
            userToken.ExpiresAt = expiresAt;
        }

        else
        {
            var NewToken = new GoogleToken
            {
                UserId = userId,
                AccessToken = credential.Token.AccessToken,
                RefreshToken = credential.Token.RefreshToken,
                ExpiresAt = expiresAt
            };
        }

        _repositoryManager.GoogleToken.UpdateToken(userToken!);
        await _repositoryManager.SaveAsync();

    }

    private async Task<UserCredential> GetUserCredentialAsync()
    {
        var clientId = _config["GmailClientId"];
        var ClientSecret = _config["GmailClientSecret"];

        return await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = ClientSecret
            },
            new[] { GmailService.Scope.GmailReadonly },
            "user",
            CancellationToken.None,
            new NullDataStore()
        );
    }
}
