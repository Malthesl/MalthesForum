using Entities;

namespace RepositoryContracts;

public interface IReactionRepository
{
    Task<Reaction> AddAsync(Reaction reaction);
    Task DeleteAsync(Reaction reaction);
    Task DeleteAllAsync(Post post);
    Task DeleteAllAsync(User user);
    Task<int> GetTotalOfTypeAsync(Post post, string type);
    IQueryable<Reaction> GetMany();
}