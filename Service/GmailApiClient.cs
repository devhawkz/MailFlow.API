using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Service.Contracts;
using Contracts;

namespace Service;
public class GmailApiClient : IGmailApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILoggerManager _logger;
    public GmailApiClient(HttpClient httpClient, ILoggerManager logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GetAsync(string path, string accessToken)
    {
        _logger.LogDebug("Sending GET request to Gmail API. Path: {Path}", path);

        var request = new HttpRequestMessage(HttpMethod.Get, path);
       request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
       request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        _logger.LogDebug("Gmail API responded. Status code: {StatusCode}", response.StatusCode);


        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarn("Gmail API request failed. Status: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase!);
            return string.Empty;
        }
            

        var content = await response.Content.ReadAsStringAsync();
        return content is not null
            ? content
            : string.Empty;
    }
}

