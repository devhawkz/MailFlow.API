using Contracts;
using Entities.Models;
using Google.Apis.Auth.OAuth2;
using Service.Contracts;
using Shared.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Service;

internal sealed class GmailLabelService : IGmailLabelService
{
    private readonly IRepositoryManager _repositoryManager;

    public GmailLabelService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    async Task<GmailLabelListDTO> IGmailLabelService.GetLabelsFromAPI(bool trackChanges)
    {
        var token = _repositoryManager.GoogleToken.GetLatestTokenForUserAsync(trackChanges);
        if (token is null)
            return new GmailLabelListDTO(Enumerable.Empty<GmailLabelDTO>());

        if (token.ExpiresAt < DateTime.UtcNow)
        {
            var credential = await _repositoryManager.Tools.GetUserCredentialAsync();
            await credential.RefreshTokenAsync(CancellationToken.None);

            token.AccessToken = credential.Token.AccessToken;
            token.RefreshToken = credential.Token.RefreshToken;
            token.ExpiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);

             _repositoryManager.GoogleToken.UpdateTokenAsync(token);
        }

        using var httpClient = new HttpClient(); 
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var response = await httpClient.GetAsync("https://gmail.googleapis.com/gmail/v1/users/me/labels");
        if (!response.IsSuccessStatusCode)
            return new GmailLabelListDTO(Enumerable.Empty<GmailLabelDTO>());

        var content = await response.Content.ReadAsStringAsync();

        var labelList = JsonSerializer.Deserialize<GmailLabelListDTO>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        foreach(var label in labelList?.Labels!)
        {
            var exists = _repositoryManager.GmailLabel.LabelExistsAsync(labelId: label.Id, userId: token.UserId, trackChanges: trackChanges);
            if(exists)
                continue;

            var gmailLabel = new GmailLabel
            {
                Id = label.Id,
                Name = label.Name,
                Type = label.Type,
                UserId = token.UserId
            };
            _repositoryManager.GmailLabel.AddLabelAsync(gmailLabel);

        }

        _repositoryManager.Save();

        return labelList;
    }
}
