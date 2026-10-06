using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Bunq.Core.Common;
using Bunq.Core.Interfaces;
using Bunq.Core.Models;
using Bunq.Infrastructure.Models;

namespace Bunq.Infrastructure.Services.BunqApi;

public sealed class BunqApiClientService
{
    private const string AuthenticationHeader = "X-Bunq-Client-Authentication";
    private const string ClientSignatureHeader = "X-Bunq-Client-Signature";
    private const string ServerSignatureHeader = "X-Bunq-Server-Signature";
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
    private readonly IRsaService _rsaService;

    public BunqApiClientService(HttpClient httpClient, IRsaService rsaService)
    {
        _httpClient = httpClient;
        _rsaService = rsaService;
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

    private static void AddCommonHeaders(HttpRequestMessage request, string? authToken = null)
    {
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
    }

    private static HttpRequestMessage CreateUnsignedJsonRequest(HttpMethod method, string endpoint, object payload, string? authToken = null)
    {
        var json = JsonSerializer.Serialize(payload);
        var request = new HttpRequestMessage(method, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        AddCommonHeaders(request, authToken);
        return request;
    }

    private HttpRequestMessage CreateSignedJsonRequest(
        HttpMethod method,
        string endpoint,
        object payload,
        string authToken,
        string privateKeyPem)
    {
        var json = JsonSerializer.Serialize(payload);
        var signature = _rsaService.SignData(json, privateKeyPem);
        var request = new HttpRequestMessage(method, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        AddCommonHeaders(request, authToken);
        request.Headers.Add(ClientSignatureHeader, signature);
        return request;
    }

    private async Task<(bool IsValid, string Body, string? Error)> TryReadAndVerifyAuthenticatedResponseAsync(
        HttpResponseMessage response,
        string serverPublicKeyPem,
        CancellationToken ct)
    {
        var responseBody = await response.Content.ReadAsStringAsync(ct);
        if (!response.Headers.TryGetValues(ServerSignatureHeader, out var signatureValues))
        {
            return (false, responseBody, "Missing bunq server signature header.");
        }

        var signature = signatureValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature))
        {
            return (false, responseBody, "Bunq server signature header is empty.");
        }

        if (!_rsaService.VerifyData(responseBody, signature, serverPublicKeyPem))
        {
            return (false, responseBody, "Bunq server signature verification failed.");
        }

        return (true, responseBody, null);
    }

    public async Task<OperationResult<InstallationResult>> CreateInstallationAsync(
        string clientPublicKeyPem, 
        CancellationToken ct = default)
    {
        var payload = new
        {
            client_public_key = clientPublicKeyPem
        };

        using var request = CreateUnsignedJsonRequest(HttpMethod.Post, InstallationEndpoint, payload);
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

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(serverPublicKey))
            {
                return OperationResult<InstallationResult>.Fail(
                    MessageTypeEnum.ExternalApi,
                    "Invalid_Installation_Response",
                    "Bunq response did not include an installation token and server public key.");
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
        string privateKeyPem,
        string serverPublicKeyPem,
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

        using var request = CreateSignedJsonRequest(
            HttpMethod.Post,
            DeviceServerEndpoint,
            payload,
            installationToken,
            privateKeyPem);
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

            var verification = await TryReadAndVerifyAuthenticatedResponseAsync(response, serverPublicKeyPem, ct);
            if (!verification.IsValid)
            {
                return OperationResult.Fail(
                    MessageTypeEnum.ExternalApi,
                    "Server_Signature_Invalid",
                    verification.Error ?? "Bunq server signature verification failed.");
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
        string privateKeyPem,
        string serverPublicKeyPem,
        string apiKey, 
        CancellationToken ct = default)
    {
        var payload = new
        {
            secret = apiKey
        };

        using var request = CreateSignedJsonRequest(
            HttpMethod.Post,
            SessionServerEndpoint,
            payload,
            installationToken,
            privateKeyPem);
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

            var verification = await TryReadAndVerifyAuthenticatedResponseAsync(response, serverPublicKeyPem, ct);
            if (!verification.IsValid)
            {
                return OperationResult<SessionInfo>.Fail(
                    MessageTypeEnum.ExternalApi,
                    "Server_Signature_Invalid",
                    verification.Error ?? "Bunq server signature verification failed.");
            }

            var responseDto = JsonSerializer.Deserialize<SessionResponse>(verification.Body);
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

            if (string.IsNullOrWhiteSpace(sessionToken) || userId <= 0)
            {
                return OperationResult<SessionInfo>.Fail(
                    MessageTypeEnum.ExternalApi,
                    "Invalid_Session_Response",
                    "Bunq response did not include a valid session token and user id.");
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
