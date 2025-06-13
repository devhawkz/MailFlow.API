using Contracts;
using Entities.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Repository;

public sealed class ToolsRepository : IToolsRepository
{
   private readonly IConfiguration _config;

    public ToolsRepository(IConfiguration configuration)
    {
        _config = configuration;
    }

    public async Task<UserCredential> GetUserCredentialAsync()
    {
        var clientId = _config["GmailClientId"];
        var clientSecret = _config["GmailClientSecret"];

        return await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            new[] { GmailService.Scope.GmailReadonly },
            "user", //location where the token will be stored is associated with this identifier
            CancellationToken.None,
            new NullDataStore() // name of the folder where the token will be stored and token file location
        );
    }

    public static string DecodeBase64UrlString(string base64Url)
    {
        base64Url = base64Url.Replace('-', '+').Replace('_', '/');
        switch (base64Url.Length % 4)
        {
            case 2: base64Url += "=="; break;
            case 3: base64Url += "="; break;

            //low risk of exception because we use GmailAPI, but still need to handle it
            case 1:
                throw new FormatException("Invalid base64url string: Length modulo 4 cannot be 1.");
        }
        var bytes = Convert.FromBase64String(base64Url);
        return Encoding.UTF8.GetString(bytes);
    }

    public GoogleToken RefreshToken(UserCredential credential, GoogleToken token)
    {
        token.AccessToken = credential.Token.AccessToken;
        token.RefreshToken = credential.Token.RefreshToken;
        token.ExpiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);

        return token;

    }
}
