using System.Net;
using System.Net.Http.Json;
using Fundoo.Notes.Application.Interfaces;

namespace Fundoo.Notes.Infrastructure.Users;

public sealed class HttpUserDirectoryClient : IUserDirectoryClient
{
    private readonly HttpClient _httpClient;

    public HttpUserDirectoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Guid?> FindUserIdByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "internal/users/lookup",
                new UserLookupRequest(email),
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<UserLookupResponse>(
                    cancellationToken: cancellationToken);

            return result?.UserId;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            throw new UserDirectoryUnavailableException(
                "The User service is temporarily unavailable.",
                exception);
        }
        catch (OperationCanceledException exception)
        {
            // HttpClient timed out; the caller did not cancel the request.
            throw new UserDirectoryUnavailableException(
                "The User service request timed out.",
                exception);
        }
    }

    private sealed record UserLookupRequest(string Email);
    private sealed record UserLookupResponse(Guid UserId);
}
