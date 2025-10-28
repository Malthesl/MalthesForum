using ApiContracts;

namespace BlazorClient.Services;

public interface ISubforumService
{
    Task<SubforumDTO> GetSubforum(string url);
    Task<SubforumDTO[]> GetAllSubforums();
}