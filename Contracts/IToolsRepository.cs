using Entities.Models;
using Google.Apis.Auth.OAuth2;

namespace Contracts;

public interface IToolsRepository
{
    Task<UserCredential> GetUserCredentialAsync();
    GoogleToken RefreshToken(UserCredential credential, GoogleToken  token);
}
