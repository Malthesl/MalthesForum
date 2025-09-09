using Entities;

namespace RepositoryContracts;

public interface ISubforumRepository
{
    Task<Subforum> AddAsync(Subforum user);
    Task UpdateAsync(Subforum user);
    Task DeleteAsync(int id);
    Task<Subforum> GetAsync(int id);
    IQueryable<Subforum> GetMany();
    Task<Subforum?> GetByName(string name);
}