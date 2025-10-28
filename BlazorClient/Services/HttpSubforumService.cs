using System.Text.Json;
using ApiContracts;

namespace BlazorClient.Services;

public class HttpSubforumService(HttpClient client) : ISubforumService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<SubforumDTO> GetSubforum(string url)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"subforums/{url}");
        string response = await httpResponse.Content.ReadAsStringAsync();
        
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<SubforumDTO>(response, JsonOptions)!;
    }

    public async Task<SubforumDTO[]> GetAllSubforums()
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"subforums");
        string response = await httpResponse.Content.ReadAsStringAsync();
        
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<SubforumDTO[]>(response, JsonOptions)!;
    }
}