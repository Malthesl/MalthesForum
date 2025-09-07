using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class SubforumInMemoryRepository : ISubforumRepository
{
    private List<Subforum> subforums = new();

    public Task<Subforum> AddAsync(Subforum subforum)
    {
        subforum.Id = subforums.Any()
            ? subforums.Max(p => p.Id) + 1
            : 1;
        subforums.Add(subforum);
        return Task.FromResult(subforum);
    }

    public Task UpdateAsync(Subforum subforum)
    {
        Subforum? Subforum = subforums.SingleOrDefault(p => p.Id == subforum.Id);
        if (Subforum is null)
        {
            throw new InvalidOperationException(
                $"Subforum with ID '{subforum.Id}' not found");
        }

        subforums.Remove(Subforum);
        subforums.Add(subforum);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Subforum? subforumToRemove = subforums.SingleOrDefault(p => p.Id == id);
        if (subforumToRemove is null)
        {
            throw new InvalidOperationException(
                $"Subforum with ID '{id}' not found");
        }

        subforums.Remove(subforumToRemove);
        return Task.CompletedTask;
    }

    public Task<Subforum> GetAsync(int id)
    {
        Subforum? subforum = subforums.SingleOrDefault(p => p.Id == id);
        if (subforum is null)
        {
            throw new InvalidOperationException(
                $"Subforum with ID '{id}' not found");
        }
        return Task.FromResult(subforum);
    }

    public IQueryable<Subforum> GetMany()
    {
        return subforums.AsQueryable();
    }
}