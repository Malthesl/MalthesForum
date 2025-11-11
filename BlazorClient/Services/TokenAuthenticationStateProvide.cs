namespace BlazorClient.Services;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

public class TokenAuthenticationStateProvider(ProtectedLocalStorage ls)
    : AuthenticationStateProvider
{
    public const string TokenKey = "jwt_token";
    private AuthenticationState _state = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var result = await ls.GetAsync<string>(TokenKey);
            var token = result.Success ? result.Value : null;

            if (!string.IsNullOrEmpty(token))
            {
                var claims = ParseClaimsFromJwt(token);
                var identity =
                    new ClaimsIdentity(claims, authenticationType: "jwt", nameType: "name", roleType: "role");
                var user = new ClaimsPrincipal(identity);
                _state = new AuthenticationState(user);
            }
            else
            {
                _state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Fejl under indlæsning af token:");
            Console.WriteLine(e.Message);
            // Hvis der sker fejl under læsning, log brugeren ud
            _state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        return _state;
    }

    public async Task SignIn(string jwt)
    {
        // Gem token i session storage
        await ls.SetAsync(TokenKey, jwt);

        // Byg ny bruger ud fra tokenet
        var claims = ParseClaimsFromJwt(jwt);
        var identity = new ClaimsIdentity(claims, authenticationType: "jwt", nameType: "name", roleType: "role");
        var user = new ClaimsPrincipal(identity);

        _state = new AuthenticationState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(_state));
    }

    public async Task SignOut()
    {
        // Fjern token fra session storage
        await ls.DeleteAsync(TokenKey);

        // Log brugeren ud
        _state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(_state));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }

    public async Task<string?> GetJwtAsync()
    {
        var result = await ls.GetAsync<string>(TokenKey);
        var token = result.Success ? result.Value : null;
        return token;
    }
}