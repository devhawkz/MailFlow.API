namespace Service.Contracts;

public interface IGmailApiClient
{
    Task<string?> GetAsync(string path, string accessToken);
}
