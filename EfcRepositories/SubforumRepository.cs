using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RepositoryContracts;

namespace EfcRepositories;

public class SubforumRepository(AppContext ctx) : ISubforumRepository
{
    public async Task<Subforum> AddAsync(Subforum subforum)
    {
        // Tjek om URL'et er taget
        if (await ctx.Subforums.AnyAsync(s => s.Url == subforum.Url))
        {
            throw new InvalidOperationException("URL name already taken");
        }

        EntityEntry<Subforum> entityEntry = await ctx.Subforums.AddAsync(subforum);
        await ctx.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task UpdateAsync(Subforum subforum)
    {
        Subforum? currentSubforum = ctx.Subforums.SingleOrDefault(s => s.Id == subforum.Id);
        if (currentSubforum is null)
        {
            throw new InvalidOperationException($"Subforum with ID '{subforum.Id}' not found");
        }

        // Tjek om URL'et er taget
        if (await ctx.Subforums.AnyAsync(s => s.Url == subforum.Url && s.Id != subforum.Id))
        {
            throw new InvalidOperationException("URL name already taken");
        }

        ctx.Subforums.Update(subforum);

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Subforum? subforumToRemove = ctx.Subforums.SingleOrDefault(p => p.Id == id);
        if (subforumToRemove is null)
        {
            throw new InvalidOperationException($"Subforum with ID '{id}' not found");
        }

        ctx.Subforums.Remove(subforumToRemove);

        await ctx.SaveChangesAsync();
    }

    public async Task<Subforum> GetAsync(int id)
    {
        Subforum? subforum = await ctx.Subforums.SingleOrDefaultAsync(p => p.Id == id);
        if (subforum is null)
        {
            throw new InvalidOperationException($"Subforum with ID '{id}' not found");
        }

        return subforum;
    }

    public IQueryable<Subforum> GetMany()
    {
        return ctx.Subforums.AsQueryable();
    }

    public async Task<Subforum?> GetByName(string name)
    {
        return await ctx.Subforums.SingleOrDefaultAsync(p => p.Name == name);
    }

    public async Task<Subforum?> GetByURL(string url)
    {
        return await ctx.Subforums.SingleOrDefaultAsync(p => p.Url == url);
    }
}