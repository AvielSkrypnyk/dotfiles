using System.Net.Http.Json;
using Bunq.Core.Common;
using Bunq.Core.Interfaces;
using Bunq.Core.Models;

namespace Bunq.Infrastructure.Clients;

public sealed class BunqClient : IBunqClient
{
    private const string AuthenticationHeader = "X-Bunq-Client-Authentication";
    private const string InstallationEndpoint = "v1/installation";
    private const string DeviceServerEndpoint = "v1/device-server";
    private const string SessionServerEndpoint = "v1/session-server";

    private readonly HttpClient _httpClient;

    public BunqClient(HttpClient httpClient)
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
        catch
        {
            // Fall through if not JSON
        }

        if (!string.IsNullOrWhiteSpace(response.ReasonPhrase))
        {
            return response.ReasonPhrase;
        }

        return "HTTP error " + (int)response.StatusCode;
    }

    public async Task<OperationResult<InstallationResult>> CreateInstallationAsync(
        string clientPublicKeyPem, 
        CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                client_public_key = clientPublicKeyPem
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, InstallationEndpoint);
            request.Content = JsonContent.Create(payload);

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

            // Loop through response items to find Token and ServerPublicKey (No magic numbers!)
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
        catch (Exception ex)
        {
            return OperationResult<InstallationResult>.Fail(
                MessageTypeEnum.Exception, 
                "Installation_Exception", 
                ex.Message);
        }
    }

    public async Task<OperationResult> RegisterDeviceAsync(
        string installationToken, 
        string apiKey, 
        string description = "Bunq CLI Suite", 
        CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                description = description,
                secret = apiKey,
                permitted_ips = Array.Empty<string>()
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, DeviceServerEndpoint);
            request.Content = JsonContent.Create(payload);
            request.Headers.Add(AuthenticationHeader, installationToken);

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
        catch (Exception ex)
        {
            return OperationResult.Fail(
                MessageTypeEnum.Exception, 
                "Device_Registration_Exception", 
                ex.Message);
        }
    }

    public async Task<OperationResult<SessionInfo>> CreateSessionAsync(
        string installationToken, 
        string apiKey, 
        CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                secret = apiKey
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, SessionServerEndpoint);
            request.Content = JsonContent.Create(payload);
            request.Headers.Add(AuthenticationHeader, installationToken);

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
        catch (Exception ex)
        {
            return OperationResult<SessionInfo>.Fail(
                MessageTypeEnum.Exception, 
                "Session_Exception", 
                ex.Message);
        }
    }
}
