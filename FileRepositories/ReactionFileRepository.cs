using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class ReactionFileRepository : IReactionRepository
{
    private readonly string _filePath = "reactions.json";

    public ReactionFileRepository()
    {
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    private async Task<List<Reaction>> GetReactionsFromFile()
    {
        string reactionsAsJson = await File.ReadAllTextAsync(_filePath);
        List<Reaction> reactions = JsonSerializer.Deserialize<List<Reaction>>(reactionsAsJson)!;
        return reactions;
    }

    private async Task SavePostsToFile(List<Reaction> reactions)
    {
        string reactionsAsJson = JsonSerializer.Serialize(reactions);
        await File.WriteAllTextAsync(_filePath, reactionsAsJson);
    }

    public async Task<Reaction> AddAsync(Reaction reaction)
    {
        List<Reaction> reactions = await GetReactionsFromFile();

        if (reactions.Any(r => r.PostId == reaction.PostId && r.UserId == reaction.UserId && r.Type == reaction.Type))
        {
            throw new InvalidOperationException("Reaction already exists");
        }

        reactions.Add(reaction);
        await SavePostsToFile(reactions);

        return reaction;
    }

    public async Task DeleteAsync(Reaction reaction)
    {
        List<Reaction> reactions = await GetReactionsFromFile();

        Reaction? r = reactions.SingleOrDefault(r =>
            r.PostId == reaction.PostId && r.UserId == reaction.UserId && r.Type == reaction.Type);

        if (r == null)
        {
            throw new InvalidOperationException("Reaction does not exist");
        }

        reactions.Remove(r);

        await SavePostsToFile(reactions);
    }

    public async Task DeleteAllByPostAsync(int postId)
    {
        List<Reaction> reactions = await GetReactionsFromFile();

        reactions.RemoveAll(reaction => reaction.PostId == postId);

        await SavePostsToFile(reactions);
    }

    public async Task DeleteAllByUserAsync(int userId)
    {
        List<Reaction> reactions = await GetReactionsFromFile();

        reactions.RemoveAll(reaction => reaction.UserId == userId);

        await SavePostsToFile(reactions);
    }

    public async Task<int> GetTotalOfTypeAsync(int postId, string type)
    {
        List<Reaction> reactions = await GetReactionsFromFile();
        return reactions.Count(r => r.PostId == postId && r.Type == type);
    }

    public async Task<Dictionary<string, int>> GetTotalOfEachTypeAsync(int postId)
    {
        List<Reaction> reactions = await GetReactionsFromFile();
        return reactions
            .Where(r => r.PostId == postId)
            .GroupBy(r => r.Type)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );
    }

    public async Task<List<string>> GetReactionsOnPostByUser(int postId, int? userId)
    {
        List<Reaction> reactions = await GetReactionsFromFile();
        return reactions.Where(r => r.PostId == postId && r.UserId == userId).Select(r => r.Type).ToList();
    }

    public IQueryable<Reaction> GetMany()
    {
        string reactionsAsJson = File.ReadAllTextAsync(_filePath).Result;
        List<Reaction> reactions = JsonSerializer.Deserialize<List<Reaction>>(reactionsAsJson)!;
        return reactions.AsQueryable();
    }
}