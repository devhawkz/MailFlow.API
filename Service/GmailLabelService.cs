using Contracts;
using Entities.Models;
using Google.Apis.Auth.OAuth2;
using Service.Contracts;
using Shared.DTOs;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service;

internal sealed class GmailLabelService : IGmailLabelService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IToolsService _toolsService;
    private readonly ILoggerManager _logger;

    public GmailLabelService(IRepositoryManager repositoryManager, IToolsService toolsService, ILoggerManager logger)
    {
        _repositoryManager = repositoryManager;
        _toolsService = toolsService;
        _logger = logger;
    }

    public async Task<bool> DownloadAndSyncLabelsAsync(bool trackChanges, string path)
    {
        _logger.LogInfo("Starting Gmail labels download and synchronization.");
        
        var token = await _toolsService.GetUserTokenAsync(trackChanges);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            _logger.LogWarn("No valid access token found. Aborting sync.");
            return false;
        }
            

        var labelList = await GetDeserializedResponseFromApi(accessToken: token.AccessToken, path: path);
        if(labelList is null || labelList.Labels is null || !labelList.Labels.Any())
        {
            _logger.LogInfo("No labels found in Gmail API response. Nothing to sync.");
            return false;
        }

        await AddLabelsToDb(labelList: labelList, userId: token.UserId, trackChanges: trackChanges);

        await _repositoryManager.SaveAsync();
        
        _logger.LogInfo("Successfully synced {Count} labels for UserId: {UserId}.", labelList.Labels.Count(), token.UserId);
        
        return true;
    }

    private async Task<GmailLabelListDTO> GetDeserializedResponseFromApi(string accessToken, string path)
    {
        _logger.LogDebug("Fetching Gmail API response for path: {Path}", path);

        var content = await _toolsService.GetHttpResponseBody(path: path, accessToken: accessToken);
        if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
        {
            _logger.LogDebug("Gmail API response is empty for path: {Path}", path);
            return new GmailLabelListDTO(Enumerable.Empty<GmailLabelDTO>());
        } 

        var labelList = JsonSerializer.Deserialize<GmailLabelListDTO>(
            content, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        _logger.LogDebug("Deserialization completed for Gmail labels.");

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
