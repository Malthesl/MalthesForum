using ApiContracts;

namespace BlazorClient.Services;

public class Pagination<T>
{
    public Pagination(QueryResponseDTO<T> response, int pageSize)
    {
        TotalResults = response.TotalResults;
        StartIndex = response.StartIndex;
        EndIndex = response.EndIndex;
        Results = response.Results;
        PageSize = pageSize;
    }
    
    public int TotalResults { get; init; }
    public int StartIndex { get; init; }
    public int EndIndex { get; init; }
    public T[] Results { get; init; }
    public int PageSize { get; init; }
    public int PageCount => (int)Math.Ceiling((double)TotalResults / PageSize);
    public int CurrentPage => StartIndex / PageSize + 1;
} 

public interface IPostService
{
    Task<PostDTO> GetPost(int id);
    Task<Pagination<PostDTO>> GetPostsFromSubforum(string subforumUrl, int pageNumber, int pageSize);
    Task<PostDTO> CreatePost(int subforumId, string title, string body);
}