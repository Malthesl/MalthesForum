using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(IUserRepository users) : ControllerBase
{
    /// <summary>
    /// Returner brugere som passer til søgekriterier
    /// </summary>
    /// <param name="search">Inkluder kun brugere hvis navn matcher søgeordet</param>
    /// <param name="limit">Angiv et maks mængde resultater</param>
    /// <param name="offset">Offset indeks for det første resultat</param>
    [HttpGet]
    public async Task<ActionResult> GetUsers([FromQuery] string search, [FromQuery] int limit = 100,
        [FromQuery] int offset = 0)
    {
        try
        {
            IQueryable<User> query = users.GetMany();

            query = query.Where(u => u.Username.Contains(search, StringComparison.CurrentCultureIgnoreCase));

            int count = query.Count();

            return Ok(new QueryDTO<UserDTO>
            {
                TotalResults = count,
                StartIndex = offset,
                EndIndex = Math.Min(offset + limit, count),
                Results = query.Skip(offset).Take(limit).Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username
                }).ToArray()
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Hent oplysninger om en bestemt bruger
    /// </summary>
    /// <param name="userId">ID'et på brugeren</param>
    [HttpGet("{userId:int}")]
    public async Task<ActionResult> GetUser([FromRoute] int userId)
    {
        try
        {
            User user = await users.GetAsync(userId);

            return Ok(new UserDTO
            {
                Id = user.Id,
                Username = user.Username
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Opret en bruger
    /// </summary>
    /// <param name="createDTO">Oplysninger om brugeren</param>
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] CreateUserDTO createDTO)
    {
        try
        {
            await users.AddAsync(
                new User
                {
                    Username = createDTO.Username,
                    Password = createDTO.Password
                }
            );

            return Ok("User created");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Rediger en bruger
    /// </summary>
    /// <param name="userId">ID'et på brugeren</param>
    /// <param name="updateDTO">Oplysninger om ændringer og brugerens login-oplysinger</param>
    [HttpPost("{userId:int}")]
    public async Task<ActionResult> UpdateUser([FromRoute] int userId, [FromBody] UpdateUserDTO updateDTO)
    {
        try
        {
            // Tjek login oplysninger
            User? user = await users.VerifyUserCredentials(updateDTO.Auth.Username, updateDTO.Auth.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");
            // TODO: Måske der skal være nogle admins der har rettighed til at rediger alle?
            if (user.Id != userId) return Unauthorized("Du har ikke rettigheder til at ændre denne bruger");

            if (updateDTO.Username is not null) user.Username = updateDTO.Username;
            if (updateDTO.Password is not null) user.Password = updateDTO.Password;

            await users.UpdateAsync(user);

            return Ok(new UserDTO
            {
                Id = user.Id,
                Username = user.Username
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Slet en bruger
    /// </summary>
    /// <param name="userId">ID'et på brugeren</param>
    /// <param name="authDTO">Login-oplysninger</param>
    [HttpDelete("{userId:int}")]
    public async Task<ActionResult> DeleteUser([FromRoute] int userId, [FromBody] UserLoginDTO authDTO)
    {
        try
        {
            // Tjek login oplysninger
            User? user = await users.VerifyUserCredentials(authDTO.Username, authDTO.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");
            if (user.Id != userId) return Unauthorized("Du har ikke rettigheder til at slette denne bruger");

            await users.DeleteAsync(userId);

            // TODO: Slet brugerens opslag? Måske man ikke må slette så længe man er moderator?

            return Ok(new UserDTO
            {
                Id = user.Id,
                Username = user.Username
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}