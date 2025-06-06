using Google.Apis.Auth.OAuth2;

namespace Contracts;

public interface IToolsRepository
{
    Task<UserCredential> GetUserCredentialAsync();
}
