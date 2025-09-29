using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class PostFileRepository : IPostRepository
{
    private readonly string _filePath = "posts.json";

    public PostFileRepository()
    {
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    private async Task<List<Post>> GetPostsFromFile()
    {
        string postsAsJson = await File.ReadAllTextAsync(_filePath);
        List<Post> posts = JsonSerializer.Deserialize<List<Post>>(postsAsJson)!;
        return posts;
    }

    private async Task SavePostsToFile(List<Post> posts)
    {
        string postsAsJson = JsonSerializer.Serialize(posts);
        await File.WriteAllTextAsync(_filePath, postsAsJson);
    }

    public async Task<Post> AddAsync(Post post)
    {
        List<Post> posts = await GetPostsFromFile();
        
        int maxId = posts.Count != 0 ? posts.Max(c => c.Id) : 1;
        post.Id = maxId + 1;
        posts.Add(post);
        
        await SavePostsToFile(posts);
        
        return post;
    }

    public async Task UpdateAsync(Post post)
    {
        List<Post> posts = await GetPostsFromFile();
        
        Post? existingPost = posts.SingleOrDefault(p => p.Id == post.Id);
        if (existingPost is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{post.Id}' not found");
        }

        posts.Remove(existingPost);
        posts.Add(post);
        
        await SavePostsToFile(posts);
    }

    public async Task DeleteAsync(int id)
    {
        List<Post> posts = await GetPostsFromFile();
        
        Post? postToRemove = posts.SingleOrDefault(p => p.Id == id);
        if (postToRemove is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        posts.Remove(postToRemove);
        
        await SavePostsToFile(posts);
    }

    public async Task<Post> GetAsync(int id)
    {
        List<Post> posts = await GetPostsFromFile();
        
        Post? post = posts.SingleOrDefault(p => p.Id == id);
        if (post is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }
        
        return post;
    }

    public IQueryable<Post> GetMany()
    {
        string postsAsJson = File.ReadAllTextAsync(_filePath).Result;
        List<Post> posts = JsonSerializer.Deserialize<List<Post>>(postsAsJson)!;
        return posts.AsQueryable();
    }

    public Task<List<Post>> GetPosts(int id)
    {
        return Task.FromResult(GetMany().Where(p => p.SubforumId == id && p.CommentedOnPostId == null).ToList());
    }

    public Task<List<Post>> GetComments(int id)
    {
        return Task.FromResult(GetMany().Where(p => p.CommentedOnPostId == id).ToList());
    }
}