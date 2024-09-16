﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Bangumi.OAuth;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Bangumi;

public partial class BangumiApi(IHttpClientFactory httpClientFactory, OAuthStore store, ILogger<BangumiApi> logger)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly Plugin _plugin = Plugin.Instance!;

    private Task<string> SendRequest(string url, string? accessToken, CancellationToken token)
    {
        return SendRequest(new HttpRequestMessage(HttpMethod.Get, url), accessToken, token);
    }

    private async Task<string> SendRequest(HttpRequestMessage request, CancellationToken token)
    {
        return await SendRequest(request, store.GetAvailable()?.AccessToken, token);
    }

    private async Task<T?> SendRequest<T>(string url, CancellationToken token)
    {
        return await SendRequest<T>(url, store.GetAvailable()?.AccessToken, token);
    }

    private async Task<T?> SendRequest<T>(string url, string? accessToken, CancellationToken token)
    {
        var jsonString = await SendRequest(url, accessToken, token);
        return JsonSerializer.Deserialize<T>(jsonString, Options);
    }

    public HttpClient GetHttpClient()
    {
        var httpClient = httpClientFactory.CreateClient(NamedClient.Default);
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Jellyfin.Plugin.Bangumi", _plugin.Version.ToString()));
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/kookxiang/jellyfin-plugin-bangumi)"));
        httpClient.Timeout = TimeSpan.FromMilliseconds(_plugin.Configuration.RequestTimeout);
        return httpClient;
    }

    public class JsonContent : StringContent
    {
        public JsonContent(object obj) : base(JsonSerializer.Serialize(obj, Options), Encoding.UTF8, "application/json")
        {
            Headers.ContentType!.CharSet = null;
        }
    }
}