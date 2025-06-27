using Entities.Models;

namespace Contracts;

public interface IGoogleTokenRepository
{
    Task<GoogleToken> GetLatestTokenForUserAsync(bool trackChanges);
    void UpdateToken(GoogleToken token);
}
