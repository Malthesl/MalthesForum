using Entities;

namespace RepositoryContracts;

public interface IPostRepository
{
    Task<Post> AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(int id);
    Task<Post> GetAsync(int id);
    IQueryable<Post> GetMany();
    Task<List<Post>> GetPosts(int subforumId);
    Task<List<Post>> GetComments(int id);
}