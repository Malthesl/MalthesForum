using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Authorization;
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
        IQueryable<Subforum> query = subforums.GetMany();

        if (search is not null)
            query = query.Where(s => s.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase));
        if (moderatedByUserId is not null) query = query.Where(s => s.ModeratorId == moderatedByUserId);

        IEnumerable<SubforumDTO> dtos = query.Select(s => new SubforumDTO
        {
            Id = s.Id,
            Name = s.Name,
            URL = s.Url,
            Moderator = new UserDTO
            {
                Id = s.ModeratorId,
                Username = users.GetAsync(s.ModeratorId).Result.Username
            },
            PostsCount = posts.GetTotalPosts(s.Id).Result
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Henter oplysninger om et subforum
    /// </summary>
    /// <param name="id">ID'et på subforummet</param>
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetSubforum([FromRoute] int id)
    {
        Subforum s = await subforums.GetAsync(id);

        return Ok(new SubforumDTO
        {
            Id = s.Id,
            Name = s.Name,
            URL = s.Url,
            Moderator = new UserDTO
            {
                Id = s.ModeratorId,
                Username = (await users.GetAsync(s.ModeratorId)).Username
            },
            PostsCount = posts.GetTotalPosts(s.Id).Result
        });
    }

    /// <summary>
    /// Henter et subforums id efter dens url navn
    /// </summary>
    /// <param name="urlName">URL navnet på et subforum</param>
    [HttpGet("{urlName}")]
    public async Task<ActionResult> GetSubforumByUrlName([FromRoute] string urlName)
    {
        Subforum? s = await subforums.GetByURL(urlName);

        if (s is null) return NotFound("Subforum findes ikke :(");

        return await GetSubforum(s.Id);
    }

    /// <summary>
    /// Opret et subforum
    /// </summary>
    /// <param name="updateDTO">DTO oplysninger om subforummet og loginoplysninger</param>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateSubforum([FromBody] CreateSubforumDTO updateDTO)
    {
        string userIdClaim = User.FindFirst("Id")!.Value;
        int userId = int.Parse(userIdClaim);

        Subforum newSubforum = new Subforum
        {
            Name = updateDTO.Name,
            Url = updateDTO.URL,
            ModeratorId = userId
        };

        await subforums.AddAsync(newSubforum);

        return Created("/subforums/" + newSubforum.Id, new SubforumDTO
        {
            Id = newSubforum.Id,
            Name = newSubforum.Name,
            URL = newSubforum.Url,
            Moderator = new UserDTO
            {
                Id = newSubforum.ModeratorId,
                Username = (await users.GetAsync(newSubforum.ModeratorId)).Username
            },
            PostsCount = 0
        });
    }

    /// <summary>
    /// Opdater oplysninger om et subforum
    /// </summary>
    /// <param name="id">ID'et på subforummet</param>
    /// <param name="updateDTO">DTO med hvad der skal ændres (navn, url) og loginoplysninger</param>
    [HttpPost("{id:int}")]
    [Authorize]
    public async Task<ActionResult> UpdateSubforum([FromRoute] int id, [FromBody] UpdateSubforumDTO updateDTO)
    {
        string userIdClaim = User.FindFirst("Id")!.Value;
        int userId = int.Parse(userIdClaim);

        // Tjek om brugeren har rettighed til at ændre subforummet
        Subforum subforum = await subforums.GetAsync(id);
        if (subforum.ModeratorId != userId)
            return Unauthorized("Du har ikke rettighed til at ændre dette subforum");

        if (updateDTO.Name is not null) subforum.Name = updateDTO.Name;
        if (updateDTO.URL is not null) subforum.Url = updateDTO.URL;
        if (updateDTO.ModeratorId is not null) subforum.ModeratorId = updateDTO.ModeratorId ?? -1;

        await subforums.UpdateAsync(subforum);

        return Ok(new SuccessDTO("Subforummet blev ændret"));
    }

    /// <summary>
    /// Slet et subforum
    /// </summary>
    /// <param name="id">ID'et på subforummet</param>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteSubforum([FromRoute] int id)
    {
        string userIdClaim = User.FindFirst("Id")!.Value;
        int userId = int.Parse(userIdClaim);

        // Tjek om brugeren har rettighed til at slette subforummet
        Subforum subforum = await subforums.GetAsync(id);
        if (subforum.ModeratorId != userId)
            return Unauthorized("Du har ikke rettighed til at slette dette subforum");

        await subforums.DeleteAsync(id);

        // TODO: Burde nok også slette alle opslag i subforummet

        return Ok(new SuccessDTO("Subforummet blev slettet"));
    }
}