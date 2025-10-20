using Entities;

namespace RepositoryContracts;

public interface IReactionRepository
{
    Task<Reaction> AddAsync(Reaction reaction);
    Task DeleteAsync(Reaction reaction);
    Task DeleteAllByPostAsync(int postId);
    Task DeleteAllByUserAsync(int userId);
    Task<int> GetTotalOfTypeAsync(int postId, string type);
    Task<Dictionary<string, int>> GetTotalOfEachTypeAsync(int postId);
    IQueryable<Reaction> GetMany();
}