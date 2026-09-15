using System.Net.Http.Json;
using System.Text.Json;
using Bunq.Core.Common;
using Bunq.Core.Models;
using Bunq.Infrastructure.Models;

namespace Bunq.Infrastructure.Services.BunqApi;

public sealed class BunqApiClientService
{
    private const string AuthenticationHeader = "X-Bunq-Client-Authentication";
    private const string RequestIdHeader = "X-Bunq-Client-Request-Id";
    private const string GeolocationHeader = "X-Bunq-Geolocation";
    private const string LanguageHeader = "X-Bunq-Language";
    private const string RegionHeader = "X-Bunq-Region";
    private const string UserAgentHeader = "User-Agent";
    private const string CacheControlHeader = "Cache-Control";
    private const string InstallationEndpoint = "v1/installation";
    private const string DeviceServerEndpoint = "v1/device-server";
    private const string SessionServerEndpoint = "v1/session-server";

    private readonly HttpClient _httpClient;

    public BunqApiClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static async Task<string> GetErrorMessageAsync(
        HttpResponseMessage response, 
        CancellationToken ct)
    {
        try
        {
            var errorDto = await response.Content.ReadFromJsonAsync<BunqErrorResponse>(cancellationToken: ct);
            if (errorDto?.Error is not null)
            {
                var firstError = errorDto.Error.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(firstError?.Description))
                {
                    return firstError.Description;
                }
            }
        }
        catch (JsonException)
        {
            // Fall through if body is not the bunq error JSON format.
        }
        catch (NotSupportedException)
        {
            // Fall through if content type is unsupported.
        }

        if (!string.IsNullOrWhiteSpace(response.ReasonPhrase))
        {
            return response.ReasonPhrase;
        }

        return "HTTP error " + (int)response.StatusCode;
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, object payload, string? authToken = null)
    {
        var request = new HttpRequestMessage(method, endpoint)
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.Add(RequestIdHeader, Guid.NewGuid().ToString());
        request.Headers.Add(GeolocationHeader, "0 0 0 0 000");
        request.Headers.Add(LanguageHeader, "en_US");
        request.Headers.Add(RegionHeader, "nl_NL");
        request.Headers.Add(UserAgentHeader, "bunq-csharp-cli/1.0");
        request.Headers.Add(CacheControlHeader, "no-cache");

        if (!string.IsNullOrWhiteSpace(authToken))
        {
            request.Headers.Add(AuthenticationHeader, authToken);
        }

        return request;
    }

    public async Task<OperationResult<InstallationResult>> CreateInstallationAsync(
        string clientPublicKeyPem, 
        CancellationToken ct = default)
    {
        var payload = new
        {
            client_public_key = clientPublicKeyPem
        };

        using var request = CreateRequest(HttpMethod.Post, InstallationEndpoint, payload);
        try
        {
            using var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await GetErrorMessageAsync(response, ct);
                return OperationResult<InstallationResult>.Fail(
                    MessageTypeEnum.ExternalApi, 
                    "Installation_Failed", 
                    error);
            }

            var responseDto = await response.Content.ReadFromJsonAsync<InstallationResponse>(cancellationToken: ct);
            if (responseDto?.Response is null)
            {
                return OperationResult<InstallationResult>.Fail(
                    MessageTypeEnum.ExternalApi, 
                    "Empty_Response", 
                    "Bunq returned an empty response.");
            }

            var token = "";
            var serverPublicKey = "";

            // Loop through response items to find Token and ServerPublicKey
            foreach (var item in responseDto.Response)
            {
                if (item.Token is not null)
                {
                    token = item.Token.Token ?? "";
                }

                if (item.ServerPublicKey is not null)
                {
                    serverPublicKey = item.ServerPublicKey.ServerPublicKey ?? "";
                }
            }

            var result = new InstallationResult(token, serverPublicKey);
            return OperationResult<InstallationResult>.Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return OperationResult<InstallationResult>.Fail(
                MessageTypeEnum.Exception,
                "Installation_Http_Exception",
                ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            return OperationResult<InstallationResult>.Fail(
                MessageTypeEnum.Exception,
                "Installation_Timeout",
                ex.Message);
        }
        catch (JsonException ex)
        {
            return OperationResult<InstallationResult>.Fail(
                MessageTypeEnum.Exception,
                "Installation_Json_Exception",
                ex.Message);
        }
    }

    public async Task<OperationResult> RegisterDeviceAsync(
        string installationToken, 
        string apiKey, 
        string description = "Bunq CLI Suite",
        string[]? permittedIps = null,
        CancellationToken ct = default)
    {
        var resolvedIps = permittedIps is { Length: > 0 } ? permittedIps : Array.Empty<string>();
        var payload = new
        {
            description = description,
            secret = apiKey,
            permitted_ips = resolvedIps
        };

        using var request = CreateRequest(HttpMethod.Post, DeviceServerEndpoint, payload, installationToken);
        try
        {
            using var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await GetErrorMessageAsync(response, ct);
                return OperationResult.Fail(
                    MessageTypeEnum.ExternalApi, 
                    "Device_Registration_Failed", 
                    error);
            }

            return OperationResult.Ok();
        }
        catch (HttpRequestException ex)
        {
            return OperationResult.Fail(
                MessageTypeEnum.Exception,
                "Device_Registration_Http_Exception",
                ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            return OperationResult.Fail(
                MessageTypeEnum.Exception,
                "Device_Registration_Timeout",
                ex.Message);
        }
        catch (JsonException ex)
        {
            return OperationResult.Fail(
                MessageTypeEnum.Exception,
                "Device_Registration_Json_Exception",
                ex.Message);
        }
    }

    public async Task<OperationResult<SessionInfo>> CreateSessionAsync(
        string installationToken, 
        string apiKey, 
        CancellationToken ct = default)
    {
        var payload = new
        {
            secret = apiKey
        };

        using var request = CreateRequest(HttpMethod.Post, SessionServerEndpoint, payload, installationToken);
        try
        {
            using var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await GetErrorMessageAsync(response, ct);
                return OperationResult<SessionInfo>.Fail(
                    MessageTypeEnum.ExternalApi, 
                    "Session_Failed", 
                    error);
            }

            var responseDto = await response.Content.ReadFromJsonAsync<SessionResponse>(cancellationToken: ct);
            if (responseDto?.Response is null)
            {
                return OperationResult<SessionInfo>.Fail(
                    MessageTypeEnum.ExternalApi, 
                    "Empty_Response", 
                    "Bunq returned an empty response.");
            }

            var sessionToken = "";
            var userId = 0;

            // Loop through response items to find Token and UserId (No magic numbers!)
            foreach (var item in responseDto.Response)
            {
                if (item.Token is not null)
                {
                    sessionToken = item.Token.Token ?? "";
                }

                if (item.UserPerson is not null)
                {
                    userId = item.UserPerson.Id;
                }
                else if (item.UserCompany is not null)
                {
                    userId = item.UserCompany.Id;
                }
                else if (item.UserApiKey is not null)
                {
                    userId = item.UserApiKey.Id;
                }
            }

            var sessionInfo = new SessionInfo
            {
                SessionToken = sessionToken,
                UserId = userId
            };

            return OperationResult<SessionInfo>.Ok(sessionInfo);
        }
        catch (HttpRequestException ex)
        {
            return OperationResult<SessionInfo>.Fail(
                MessageTypeEnum.Exception,
                "Session_Http_Exception",
                ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            return OperationResult<SessionInfo>.Fail(
                MessageTypeEnum.Exception,
                "Session_Timeout",
                ex.Message);
        }
        catch (JsonException ex)
        {
            return OperationResult<SessionInfo>.Fail(
                MessageTypeEnum.Exception,
                "Session_Json_Exception",
                ex.Message);
        }
    }
}
