using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SubforumsController(
    ISubforumRepository subforums,
    IPostRepository posts,
    IUserRepository users) : ControllerBase
{
    /// <summary>
    /// Query subforums efter et søgeord eller efter moderator id
    /// </summary>
    /// <param name="search">Inkluder kun subforums hvis navn matcher søgeordet</param>
    /// <param name="moderatedByUserId">Inkluder kun subforums som er modereret af en bestemt bruger</param>
    [HttpGet]
    public async Task<ActionResult> GetSubforums([FromQuery] string? search, [FromQuery] int? moderatedByUserId)
    {
        try
        {
            IQueryable<Subforum> query = subforums.GetMany();

            if (search is not null)
                query = query.Where(s => s.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase));
            if (moderatedByUserId is not null) query = query.Where(s => s.ModeratorUserId == moderatedByUserId);

            IEnumerable<SubforumDTO> dtos = query.Select(s => new SubforumDTO
            {
                Id = s.Id,
                Name = s.Name,
                URL = s.URL,
                Moderator = new UserDTO
                {
                    Id = s.ModeratorUserId,
                    Username = users.GetAsync(s.ModeratorUserId).Result.Username
                },
                PostsCount = posts.GetTotalPosts(s.Id).Result
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Henter oplysninger om et subforum
    /// </summary>
    /// <param name="id">ID'et på subforummet</param>
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetSubforum([FromRoute] int id)
    {
        try
        {
            Subforum s = await subforums.GetAsync(id);

            return Ok(new SubforumDTO
            {
                Id = s.Id,
                Name = s.Name,
                URL = s.URL,
                Moderator = new UserDTO
                {
                    Id = s.ModeratorUserId,
                    Username = users.GetAsync(s.ModeratorUserId).Result.Username
                },
                PostsCount = posts.GetTotalPosts(s.Id).Result
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Henter et subforums id efter dens url navn
    /// </summary>
    /// <param name="urlName">URL navnet på et subforum</param>
    [HttpGet("{urlName}")]
    public async Task<ActionResult> GetSubforumByUrlName([FromRoute] string urlName)
    {
        try
        {
            Subforum s = await subforums.GetByURL(urlName);

            if (s is null) return NotFound("Subforum findes ikke :(");

            return await GetSubforum(s.Id);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Opret et subforum
    /// </summary>
    /// <param name="updateDTO">DTO oplysninger om subforummet og loginoplysninger</param>
    [HttpPost]
    public async Task<ActionResult> UpdateSubforum([FromBody] CreateSubforumDTO updateDTO)
    {
        try
        {
            // Tjek login oplysninger
            User? user = await users.VerifyUserCredentials(updateDTO.Auth.Username, updateDTO.Auth.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");

            Subforum newSubforum = new Subforum
            {
                Name = updateDTO.Name,
                URL = updateDTO.URL,
                ModeratorUserId = user.Id
            };

            await subforums.AddAsync(newSubforum);

            return Created("/subforums/" + newSubforum.Id, new SubforumDTO
            {
                Id = newSubforum.Id,
                Name = newSubforum.Name,
                URL = newSubforum.URL,
                Moderator = new UserDTO
                {
                    Id = newSubforum.ModeratorUserId,
                    Username = user.Username
                },
                PostsCount = 0
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Opdater oplysninger om et subforum
    /// </summary>
    /// <param name="id">ID'et på subforummet</param>
    /// <param name="updateDTO">DTO med hvad der skal ændres (navn, url) og loginoplysninger</param>
    [HttpPost("{id:int}")]
    public async Task<ActionResult> UpdateSubforum([FromRoute] int id, [FromBody] UpdateSubforumDTO updateDTO)
    {
        try
        {
            // Tjek login oplysninger
            User? user = await users.VerifyUserCredentials(updateDTO.Auth.Username, updateDTO.Auth.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");

            // Tjek om brugeren har rettighed til at ændre subforummet
            Subforum subforum = await subforums.GetAsync(id);
            if (subforum.ModeratorUserId != user.Id)
                return Unauthorized("Du har ikke rettighed til at ændre dette subforum");

            subforum.Name = updateDTO.Name;
            subforum.URL = updateDTO.URL;
            subforum.ModeratorUserId = updateDTO.ModeratorId;

            await subforums.UpdateAsync(subforum);

            return Ok("Subforummet blev ændret");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Slet et subforum
    /// </summary>
    /// <param name="id">ID'et på subforummet</param>
    /// <param name="authDTO">Loginoplysninger</param>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteSubforum([FromRoute] int id, [FromBody] UserLoginDTO authDTO)
    {
        try
        {
            // Tjek login oplysninger
            User? user = await users.VerifyUserCredentials(authDTO.Username, authDTO.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");

            // Tjek om brugeren har rettighed til at slette subforummet
            Subforum subforum = await subforums.GetAsync(id);
            if (subforum.ModeratorUserId != user.Id)
                return Unauthorized("Du har ikke rettighed til at slette dette subforum");

            await subforums.DeleteAsync(id);

            // TODO: Burde nok også slette alle opslag i subforummet

            return Ok("Subforummet blev slettet");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}