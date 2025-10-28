using ApiContracts;

namespace BlazorClient.Services;

public interface IUserService
{
    Task<UserDTO> CreateUser(string username, string password);
}