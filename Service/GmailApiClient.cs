﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Service.Contracts;

namespace Service;
public class GmailApiClient : IGmailApiClient
{
    private readonly HttpClient _httpClient;
    public GmailApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri("https://gmail.googleapis.com/gmail/v1/users/me/");
    }

    public async Task<Stream?> GetAsync(string path, string accessToken)
    {
       var request = new HttpRequestMessage(HttpMethod.Get, path);
       request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
       request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
            return Stream.Null;

        var content = await response.Content.ReadAsStreamAsync();
        return content is null || content.Length == 0
            ? Stream.Null
            : content;
    }
}

