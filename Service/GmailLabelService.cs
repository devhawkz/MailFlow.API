using Contracts;
using Entities.Models;
using Google.Apis.Auth.OAuth2;
using Service.Contracts;
using Shared.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service;

internal sealed class GmailLabelService : IGmailLabelService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IToolsService _toolsService;
    private const string _path = "https://gmail.googleapis.com/gmail/v1/users/me/labels";
    public GmailLabelService(IRepositoryManager repositoryManager, IToolsService toolsService)
    {
        _repositoryManager = repositoryManager;
        _toolsService = toolsService;
    }

    public async Task<bool> DownloadAndSyncLabelsAsync(bool trackChanges)
    {
        var token = await _toolsService.GetUserTokenAsync(trackChanges);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            return false;

        var labelList = await GetDeserializedResponseFromApi(accessToken: token.AccessToken);
        if(labelList is null || !labelList.Labels.Any())
            return false;

        await AddLabelsToDb(labelList: labelList, userId: token.UserId, trackChanges: trackChanges);

        await _repositoryManager.SaveAsync();

        return true;
    }

    private async Task<GmailLabelListDTO> GetDeserializedResponseFromApi(string accessToken)
    {
        var content = await _toolsService.GetHttpResponseBody(path: _path, accessToken: accessToken);
        if (string.IsNullOrEmpty(content))
            return null!;

        var labelList = JsonSerializer.Deserialize<GmailLabelListDTO>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return labelList!;
    }
    private async Task AddLabelsToDb(GmailLabelListDTO labelList, Guid userId, bool trackChanges)
    {
        var labelsIds = await _repositoryManager.GmailLabel.FindExistingLabelsIdsAsync(userId: userId, trackChanges: trackChanges);
        var existingLabelIds = labelsIds.ToHashSet();
        
        var newLabels = labelList.Labels
            .Where(label => !existingLabelIds.Contains(label.Id))
            .Select(label => new GmailLabel
            {
                Id = label.Id,
                Name = label.Name,
                Type = label.Type,
                UserId = userId
            }).ToList();


        if(newLabels.Any())
            _repositoryManager.GmailLabel.AddRangeLabels(newLabels);
    }
}
