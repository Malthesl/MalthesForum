using System.Text.Json;
using ApiContracts;

namespace BlazorClient.Services;

public class HttpUserService(HttpClient client) : IUserService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<UserDTO> CreateUser(string username, string password)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync($"/users/register", new CreateUserDTO
        {
            Username = username,
            Password = password
        });

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception("HTTP FEJL: <" + httpResponse.StatusCode + "> " +
                                await httpResponse.Content.ReadAsStringAsync()); //TODO
        }

        string response = await httpResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserDTO>(response, JsonOptions)!;
    }
}