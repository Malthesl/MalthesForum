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
            throw new InvalidOperationException("Reaction already exists");
        }
        
        reactions.Remove(r);
        
        return Task.FromResult(r);
    }

    public Task DeleteAllAsync(Post post)
    {
        reactions.RemoveAll(reaction => reaction.PostId == post.Id);
        return Task.CompletedTask;
    }

    public Task DeleteAllAsync(User user)
    {
        reactions.RemoveAll(reaction => reaction.UserId == user.Id);
        return Task.CompletedTask;
    }

    public Task<int> GetTotalOfTypeAsync(Post post, string type)
    {
        return Task.FromResult(reactions.Count(r => r.PostId == post.Id && r.Type == type));
    }

    public IQueryable<Reaction> GetMany()
    {
        return reactions.AsQueryable();
    }
}