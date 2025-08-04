using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test_MauiApp1.Services;

public class TokenHttpClient
{
    //private readonly HttpClient _httpClient;
    private readonly TokenClientService _tokenClientService;
    private readonly StateService _stateService;
    private readonly IConfiguration _configuration;

    public TokenHttpClient(HttpClient httpClient, TokenClientService tokenClientService, StateService stateService, IConfiguration configuration )
    {
        //_httpClient = httpClient;
        _tokenClientService = tokenClientService;
        _stateService = stateService;
        _configuration = configuration;
    }
    

    static ConcurrentDictionary<string, SemaphoreSlim>  dic =new();
   // public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken=default,  int? listAggregationId = null)
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, int? listAggregationId = null, CancellationToken cancellationToken = default)
    {

        //await _tokenClientService.CheckAndSetNewTokens();

        var mes = new HttpClientHandler() ;
        mes.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        var _httpClient = new HttpClient(mes);
        _httpClient.BaseAddress = new Uri( _configuration.GetSection("AppSettings")["ShoppingWebAPIBaseAddress"]);
         
        if (listAggregationId is not null)
        {
           SetRequestAuthorizationLevelHeader(request, (int)listAggregationId);
        }

        var signalRId = _stateService.StateInfo.ClientSignalRID;

        request.Headers.Add("SignalRId", signalRId);

        var accessToken = _stateService.StateInfo.Token;

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = null;
        try
        {
            response = await _httpClient.SendAsync(request);//, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
        if (response.StatusCode == HttpStatusCode.Unauthorized && response.Headers.TryGetValues("Token-Expired", out var values))
        {
            await _tokenClientService.CheckAndSetNewTokens();
            accessToken = _stateService.StateInfo.Token;

            var requestClone = CloneRequest(request);
            requestClone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (listAggregationId is not null)
            {
                SetRequestAuthorizationLevelHeader(requestClone, (int)listAggregationId);
            }

            response = await _httpClient.SendAsync(requestClone);//, cancellationToken);
        }

        return response;
    }

    private HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content, // only if content may be used many times (not stream or files)
            Version = request.Version
        };

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (var property in request.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object>(property.Key), property.Value);

        return clone;
    }

    void  SetRequestAuthorizationLevelHeader(HttpRequestMessage httpRequestMessage, int listAggregationId)
    {

        if (_stateService.StateInfo.Token != null)
        {
            if (httpRequestMessage.Headers.Contains("listAggregationId"))
                httpRequestMessage.Headers.Remove("listAggregationId");

            if (httpRequestMessage.Headers.Contains("Hash"))
                httpRequestMessage.Headers.Remove("Hash");

            httpRequestMessage.Headers.Add("listAggregationId", listAggregationId.ToString());

            using SHA256 mySHA256 = SHA256.Create();

            var bytes = Encoding.ASCII.GetBytes(_stateService.StateInfo.Token + listAggregationId.ToString());

            var hashBytes = mySHA256.ComputeHash(bytes);

            var hashString = Convert.ToBase64String(hashBytes);
            
            httpRequestMessage.Headers.Add("Hash", hashString);
        }
    }
}