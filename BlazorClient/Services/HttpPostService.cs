using System.Text.Json;
using ApiContracts;

namespace BlazorClient.Services;

public class HttpPostService(HttpClient client) : IPostService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<PostDTO> GetPost(int id)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"posts/{id}?asUserId=1");
        string response = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<PostDTO>(response, JsonOptions)!;
    }

    public async Task<Pagination<PostDTO>> GetPostsFromSubforum(string subforumUrl, int pageNumber, int pageSize)
    {
        HttpResponseMessage httpResponse = await client.GetAsync(
            $"posts?subforumUrl={subforumUrl}&offset={(pageNumber - 1) * pageSize}&limit={pageSize}&type=post&asUserId=1");
        string response = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return new Pagination<PostDTO>(JsonSerializer.Deserialize<QueryResponseDTO<PostDTO>>(response, JsonOptions)!,
            pageSize);
    }

    public async Task<PostDTO> CreatePost(int subforumId, string title, string body)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync($"/posts", new CreatePostDTO
        {
            Title = title,
            Body = body,
            SubforumId = subforumId
        });

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception("HTTP FEJL: <" + httpResponse.StatusCode + "> " +
                                await httpResponse.Content.ReadAsStringAsync()); //TODO
        }

        string response = await httpResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PostDTO>(response, JsonOptions)!;
    }

    public async Task<PostDTO> PostComment(int postId, string body)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync($"/Posts/{postId}/comment", new CreateCommentDTO
        {
            Body = body
        });

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception("HTTP FEJL: <" + httpResponse.StatusCode + "> " +
                                await httpResponse.Content.ReadAsStringAsync()); //TODO
        }

        string response = await httpResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PostDTO>(response, JsonOptions)!;
    }

    public async Task AddReaction(PostDTO post, string reactionType)
    {
        if (!post.HasReacted.Contains(reactionType))
        {
            // Spørg backend
            HttpResponseMessage httpResponse =
                await client.PostAsync($"/posts/{post.Id}/react?type={reactionType}", new StringContent(""));

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception("HTTP FEJL: <" + httpResponse.StatusCode + "> " +
                                    await httpResponse.Content.ReadAsStringAsync()); //TODO
            }

            // Lav ændringen lokalt
            post.Reactions[reactionType] = post.Reactions.GetValueOrDefault(reactionType) + 1;
            post.HasReacted.Add(reactionType);
        }
    }

    public async Task RemoveReaction(PostDTO post, string reactionType)
    {
        if (post.HasReacted.Contains(reactionType))
        {
            // Spørg backend
            HttpResponseMessage httpResponse = await client.DeleteAsync($"/posts/{post.Id}/react?type={reactionType}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception("HTTP FEJL: <" + httpResponse.StatusCode + "> " +
                                    await httpResponse.Content.ReadAsStringAsync()); //TODO
            }

            // Lav ændringen lokalt
            post.Reactions[reactionType] = post.Reactions.GetValueOrDefault(reactionType) - 1;
            post.HasReacted.Remove(reactionType);
        }
    }
}