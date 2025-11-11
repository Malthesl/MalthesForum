using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorClient.Services;

/**
 * Den her DelegatingHandler tilføjer Authorization header til alle HTTP-requests hvis brugeren er logget ind.
 */
public class AuthTokenHandler(AuthenticationStateProvider authProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (authProvider is TokenAuthenticationStateProvider provider)
        {
            var token = await provider.GetJwtAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}