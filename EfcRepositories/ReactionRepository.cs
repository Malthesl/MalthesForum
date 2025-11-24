using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RepositoryContracts;

namespace EfcRepositories;

public class ReactionRepository(AppContext ctx) : IReactionRepository
{
    public async Task<Reaction> AddAsync(Reaction reaction)
    {
        if (await ctx.Reactions.AnyAsync(r =>
                r.PostId == reaction.PostId && r.UserId == reaction.UserId && r.Type == reaction.Type))
        {
            throw new InvalidOperationException("Reaction already exists");
        }

        EntityEntry<Reaction> entityEntry = await ctx.Reactions.AddAsync(reaction);
        await ctx.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task DeleteAsync(Reaction reaction)
    {
        Reaction? r = ctx.Reactions.SingleOrDefault(r =>
            r.PostId == reaction.PostId && r.UserId == reaction.UserId && r.Type == reaction.Type);

        if (r == null)
        {
            throw new InvalidOperationException("Reaction does not exist");
        }
        
        ctx.Reactions.Remove(r);
        
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAllByPostAsync(int postId)
    {
        ctx.Reactions.RemoveRange(ctx.Reactions.Where(reaction => reaction.PostId == postId));
        
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAllByUserAsync(int userId)
    {
        ctx.Reactions.RemoveRange(ctx.Reactions.Where(reaction => reaction.UserId == userId));
        
        await ctx.SaveChangesAsync();
    }

    public async Task<int> GetTotalOfTypeAsync(int postId, string type)
    {
        return await ctx.Reactions.CountAsync(r => r.PostId == postId && r.Type == type);
    }

    public async Task<Dictionary<string, int>> GetTotalOfEachTypeAsync(int postId)
    {
        return await ctx.Reactions
            .Where(r => r.PostId == postId)
            .GroupBy(r => r.Type)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Count()
            );
    }

    public async Task<List<string>> GetReactionsOnPostByUser(int postId, int? userId)
    {
        return await ctx.Reactions.Where(r => r.PostId == postId && r.UserId == userId).Select(r => r.Type).ToListAsync();
    }

    public IQueryable<Reaction> GetMany()
    {
        return ctx.Reactions.AsQueryable();
    }
}