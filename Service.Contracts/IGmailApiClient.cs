namespace Service.Contracts;

public interface IGmailApiClient
{
    Task<Stream?> GetAsync(string path, string accessToken);
}
