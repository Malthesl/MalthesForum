using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RepositoryContracts;

namespace EfcRepositories;

public class PostRepository(AppContext ctx) : IPostRepository
{
    public async Task<Post> AddAsync(Post post)
    {
        EntityEntry<Post> entityEntry = await ctx.Posts.AddAsync(post);
        await ctx.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task UpdateAsync(Post post)
    {
        if (!await ctx.Posts.AnyAsync(p => p.Id == post.Id))
        {
            throw new InvalidOperationException($"Post with ID '{post.Id}' not found");
        }

        ctx.Posts.Update(post);

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Post? existing = await ctx.Posts.SingleOrDefaultAsync(p => p.Id == id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Post with id {id} not found");
        }

        ctx.Posts.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<Post> GetAsync(int id)
    {
        Post? post = ctx.Posts
            .Include(p => p.WrittenBy)
            .Include(p => p.Subforum)
            .SingleOrDefault(p => p.Id == id);
        return post ?? throw new InvalidOperationException($"Post with ID '{id}' not found");
    }

    public IQueryable<Post> GetMany()
    {
        return ctx.Posts.AsQueryable();
    }

    public async Task<List<Post>> GetPosts(int id)
    {
        return await ctx.Posts.Where(p => p.SubforumId == id && p.CommentedOn == null).ToListAsync();
    }

    public async Task<int> GetTotalPosts(int id)
    {
        return await ctx.Posts.CountAsync(p => p.SubforumId == id && p.CommentedOn == null);
    }

    public async Task<List<Post>> GetComments(int id)
    {
        return await ctx.Posts.Where(p => p.CommentedOnId == id).ToListAsync();
    }

    public async Task<int> GetTotalComments(int id)
    {
        return await ctx.Posts.CountAsync(p => p.CommentedOnId == id);
    }
}