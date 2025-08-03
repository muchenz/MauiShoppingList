using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Test_MauiApp1.Models.Response;

namespace Test_MauiApp1.Services;

public class TokenClientService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly StateService _stateService;

    public TokenClientService(HttpClient httpClient, IConfiguration configuration, StateService stateService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _stateService = stateService;
    }
    public async Task<bool> RefreshTokensAsync()
    {
        var refreshToken = _stateService.StateInfo.RefreshToken;
        var accessToken = _stateService.StateInfo.Token;
        var expectedVersion = int.Parse(ParseClaimsFromJwt(accessToken).First(a => a.Type == ClaimTypes.Version).Value) + 1;
        Preferences.Default.Set("expectedVersion", expectedVersion);

        return await RefreshTokensAsync(accessToken, refreshToken);
    }

    private async Task<bool> RefreshTokensAsync(string accessToken, string refreshToken)
    {
             

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "User/GetNewToken");

        requestMessage.Headers.Authorization
                    = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
        requestMessage.Headers.Add("refresh_token", refreshToken);
        requestMessage.Headers.Add("deviceid", Preferences.Default.Get("gid", ""));
        
        HttpResponseMessage response = null;
            response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }


        var tokensResponse = await response.Content.ReadFromJsonAsync<UserNameAndTokensResponse>();

        SetTokens(tokensResponse.Token, tokensResponse.RefreshToken);
        return true;
    }

    SemaphoreSlim semSlim = new SemaphoreSlim(1);
    public bool IsTokenRefresing { get; set; } = false;


    public async Task CheckAndSetNewTokens()
    {
        if (!IsTokenExpired())
        {
            return;
        }
        try
        {
            await semSlim.WaitAsync();
            if (IsTokenExpired())
            {
                IsTokenRefresing = true; 
                await RefreshTokensAsync();
            }
        }
        finally
        {
            IsTokenRefresing = false;
            semSlim.Release();

        }
    }

    private void SetTokens(string accessToken, string refreshToken)
    {
        Preferences.Default.Set("Token", accessToken);
        Preferences.Default.Set("RefreshToken", refreshToken);

        _stateService.StateInfo.Token =accessToken;
        _stateService.StateInfo.RefreshToken = refreshToken;
    }
    public bool IsTokenExpired()
    {
        var token = _stateService.StateInfo.Token;
        var claims = ParseClaimsFromJwt(token);

        var claimsList = claims.ToList();   

        var expiration = claims.Where(a=>a.Type == "exp_datetime").First().Value;

        // UTC czas, więc trzeba porównać z DateTime.UtcNow
        return DateTimeOffset.Parse( expiration ) < DateTime.UtcNow;
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

        if (roles != null)
        {
            if (roles.ToString().Trim().StartsWith("["))
            {
                var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                foreach (var parsedRole in parsedRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
            }

            keyValuePairs.Remove(ClaimTypes.Role);
        }

        claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));

        // claims.Add(new Claim(ClaimTypes.Role, "admin"));


        if (keyValuePairs.TryGetValue("exp", out var expValue))
        {

            var exp = long.Parse(((JsonElement) expValue).GetString());
            var expDate = DateTimeOffset.FromUnixTimeSeconds(exp);
            claims.Add(new Claim("exp_datetime", expDate.ToUniversalTime().ToString("o")));
        }

        return claims;
    }
    private byte[] ParseBase64WithoutPadding(string base64)
    {
        //base64 = base64.Replace('-', '+').Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}