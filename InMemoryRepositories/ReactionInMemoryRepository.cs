using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class ReactionInMemoryRepository : IReactionRepository
{
    private List<Reaction> reactions = new();

    public Task<Reaction> AddAsync(Reaction reaction)
    {
        if (reactions.Any(r => r.PostId == reaction.PostId && r.UserId == reaction.UserId && r.Type == reaction.Type))
        {
            throw new InvalidOperationException("Reaction already exists");
        }

        reactions.Add(reaction);
        return Task.FromResult(reaction);
    }

    public Task DeleteAsync(Reaction reaction)
    {
        Reaction? r = reactions.SingleOrDefault(r =>
            r.PostId == reaction.PostId && r.UserId == reaction.UserId && r.Type == reaction.Type);

        if (r == null)
        {
            throw new InvalidOperationException("Reaction does not exist");
        }

        reactions.Remove(r);

        return Task.FromResult(r);
    }

    public Task DeleteAllByPostAsync(int postId)
    {
        reactions.RemoveAll(reaction => reaction.PostId == postId);
        return Task.CompletedTask;
    }

    public Task DeleteAllByUserAsync(int userId)
    {
        reactions.RemoveAll(reaction => reaction.UserId == userId);
        return Task.CompletedTask;
    }

    public Task<int> GetTotalOfTypeAsync(int postId, string type)
    {
        return Task.FromResult(reactions.Count(r => r.PostId == postId && r.Type == type));
    }

    public Task<Dictionary<string, int>> GetTotalOfEachTypeAsync(int postId)
    {
        return Task.FromResult(
            reactions
                .Where(r => r.PostId == postId)
                .GroupBy(r => r.Type)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                )
        );
    }

    public Task<List<string>> GetReactionsOnPostByUser(int postId, int? userId)
    {
        return Task.FromResult(
            reactions.Where(r => r.PostId == postId && r.UserId == userId).Select(r => r.Type).ToList()
        );
    }

    public IQueryable<Reaction> GetMany()
    {
        return reactions.AsQueryable();
    }
}