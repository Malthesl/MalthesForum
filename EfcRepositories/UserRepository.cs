using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RepositoryContracts;

namespace EfcRepositories;

public class UserRepository(AppContext ctx) : IUserRepository
{
    public async Task<User> AddAsync(User user)
    {
        // Tjek om brugernavnet er taget
        if (await ctx.Users.AnyAsync(p => p.Username == user.Username))
        {
            throw new InvalidOperationException("Username already taken");
        }

        EntityEntry<User> entityEntry = await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task UpdateAsync(User user)
    {
        User? existingUser = ctx.Users.SingleOrDefault(p => p.Id == user.Id);
        if (existingUser is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{user.Id}' not found");
        }

        // Tjek om brugernavnet er taget (ved omdøbning)
        if (await ctx.Users.AnyAsync(p => p.Username == user.Username && p.Id != user.Id))
        {
            throw new InvalidOperationException("Username already taken");
        }

        ctx.Users.Update(user);

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        User? userToRemove = await ctx.Users.SingleOrDefaultAsync(p => p.Id == id);
        if (userToRemove is null)
        {
            throw new InvalidOperationException($"User with ID '{id}' not found");
        }

        ctx.Users.Remove(userToRemove);
        
        await ctx.SaveChangesAsync();
    }

    public async Task<User> GetAsync(int id)
    {
        User? user = await ctx.Users.SingleOrDefaultAsync(p => p.Id == id);
        if (user is null)
        {
            throw new InvalidOperationException($"User with ID '{id}' not found");
        }

        return user;
    }

    public IQueryable<User> GetMany()
    {
        return ctx.Users.AsQueryable();
    }

    public async Task<User?> GetByUsername(string username)
    {
        return await ctx.Users.SingleOrDefaultAsync(p => p.Username == username);
    }

    public async Task<User?> VerifyUserCredentials(string username, string password)
    {
        return await ctx.Users.FirstOrDefaultAsync(user => user.Username == username && user.Password == password);
    }
}