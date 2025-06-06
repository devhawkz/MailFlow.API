using Entities.Models;

namespace Contracts;

public interface IGoogleTokenRepository
{
    GoogleToken GetLatestTokenForUserAsync(bool trackChanges);
    void UpdateTokenAsync(GoogleToken token);
}
