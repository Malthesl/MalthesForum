using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class UserInMemoryRepository : IUserRepository
{
    private List<User> users = new();

    public Task<User> AddAsync(User user)
    {
        user.Id = users.Count != 0
            ? users.Max(p => p.Id) + 1
            : 1;
        users.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user)
    {
        User? existingUser = users.SingleOrDefault(p => p.Id == user.Id);
        if (existingUser is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{user.Id}' not found");
        }

        users.Remove(existingUser);
        users.Add(user);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        User? userToRemove = users.SingleOrDefault(p => p.Id == id);
        if (userToRemove is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        users.Remove(userToRemove);
        return Task.CompletedTask;
    }

    public Task<User> GetAsync(int id)
    {
        User? user = users.SingleOrDefault(p => p.Id == id);
        if (user is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }
        return Task.FromResult(user);
    }

    public IQueryable<User> GetMany()
    {
        return users.AsQueryable();
    }

    public Task<User?> GetByUsername(string username)
    {
        return Task.FromResult(users.SingleOrDefault(p => p.Username == username));
    }

    public Task<User?> VerifyUserCredentials(string username, string password)
    {
        return Task.FromResult(users.FirstOrDefault(user => user.Username == username && user.Password == password));
    }
}