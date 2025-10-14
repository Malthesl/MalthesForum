using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController(
    IPostRepository posts,
    IUserRepository users,
    IReactionRepository reactions,
    ISubforumRepository subforums)
    : ControllerBase
{
    /// <summary>
    /// Hjælpemetode der opretter DTO fra et opslag
    /// </summary>
    private async Task<PostDTO> ToPostDTO(Post post, int commentDepth = 0)
    {
        User writtenBy = await users.GetAsync(post.WrittenByUserId);

        return new PostDTO
        {
            Id = post.Id,
            Body = post.Body,
            Title = post.Title,
            SubforumId = post.SubforumId,
            PostedDate = post.PostedDate,
            Edited = post.Edited,
            EditedDate = post.EditedDate,
            WrittenBy = new UserDTO
            {
                Id = writtenBy.Id,
                Username = writtenBy.Username
            },
            CommentsCount = await posts.GetTotalComments(post.Id),
            Comments = await GetCommentDTOs(post.Id, commentDepth),
            Likes = await reactions.GetTotalOfTypeAsync(post.Id, "like"),
            Dislikes = await reactions.GetTotalOfTypeAsync(post.Id, "dislike"),
        };
    }

    /// <summary>
    /// Hjælpemetode der opretter DTO fra en kommentar
    /// </summary>
    private async Task<CommentDTO> ToCommentDTO(Post comment, int commentDepth = 0)
    {
        User writtenBy = await users.GetAsync(comment.WrittenByUserId);

        return new CommentDTO
        {
            Id = comment.Id,
            Body = comment.Body,
            WrittenBy = new UserDTO
            {
                Id = comment.WrittenByUserId,
                Username = writtenBy.Username
            },
            SubforumId = comment.SubforumId,
            CommentedOnPostId = comment.CommentedOnPostId ?? -1,
            CommentsCount = await posts.GetTotalComments(comment.Id),
            Comments = await GetCommentDTOs(comment.Id, commentDepth - 1),
            Likes = await reactions.GetTotalOfTypeAsync(comment.Id, "like"),
            Dislikes = await reactions.GetTotalOfTypeAsync(comment.Id, "dislike"),
            PostedDate = comment.PostedDate,
            Edited = comment.Edited,
            EditedDate = comment.EditedDate
        };
    }

    /// <summary>
    /// Hjælpemetode til at hente kommentarer under et opslag
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="depth">Dybde at hente kommentarer</param>
    private async Task<CommentDTO[]> GetCommentDTOs(int id, int depth)
    {
        if (depth == 0) return [];

        List<Post> comments = await posts.GetComments(id);

        return await Task.WhenAll(comments.Select(comment => ToCommentDTO(comment, depth - 1)));
    }

    /// <summary>
    /// Query efter en masse forskellige parametre.
    /// </summary>
    /// <param name="subforumId">Kun inkluder opslag fra et bestemt subforum</param>
    /// <param name="userId">Kun inkluder opslag skrevet af en bestemt bruger</param>
    /// <param name="search">Kun inkluder opslag der indeholder strengen i enten titel eller body</param>
    /// <param name="before">Kun inkluder opslag fra før en dato</param>
    /// <param name="after">Kun inkluder opslag fra efter en dato</param>
    /// <param name="commentDepth">Angiv en dybde at returner kommentarer under opslag, f.eks. vil en dybde på 2 returner kommentarer på alle resultater + kommentarerene på de første kommentarer</param>
    /// <param name="limit">Angiv et maks mængde resultater</param>
    /// <param name="offset">Offset indeks for det første resultat</param>
    [HttpGet]
    public async Task<ActionResult> GetPosts([FromQuery] int? subforumId, [FromQuery] int? userId,
        [FromQuery] string? search, [FromQuery] DateTime? before, [FromQuery] DateTime? after,
        [FromQuery] int commentDepth = 0, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        try
        {
            IQueryable<Post> query = posts.GetMany();

            // Query efter de forskellige parametre
            if (subforumId is not null) query = query.Where(p => p.SubforumId == subforumId);
            if (userId is not null) query = query.Where(p => p.WrittenByUserId == userId);
            if (search is not null)
                query = query.Where(p =>
                    p.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    p.Body.Contains(search, StringComparison.CurrentCultureIgnoreCase));
            if (before is not null) query = query.Where(p => p.PostedDate < before);
            if (after is not null) query = query.Where(p => p.PostedDate > after);

            int count = query.Count();

            // Kun vælg resultater efter offset og maks limit
            query = query.Skip(offset).Take(limit);

            // Opret DTOs fra resultaterne
            PostDTO[] dtos = await Task.WhenAll(
                query.AsEnumerable().Select(post => ToPostDTO(post, commentDepth))
            );

            return Ok(new QueryDTO<PostDTO>
            {
                TotalResults = count,
                StartIndex = offset,
                EndIndex = Math.Min(offset + limit, count),
                Results = dtos
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Hent et opslag via id
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetPost([FromRoute] int id)
    {
        try
        {
            Post post = await posts.GetAsync(id);

            return Ok(await ToPostDTO(post, 3));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    ///
    /// Opret et opslag i et subforum
    ///
    [HttpPost]
    public async Task<ActionResult> CreatePost([FromBody] CreatePostDTO createDTO)
    {
        try
        {
            // Verificer brugeren og hent bruger id
            User? user = await users.VerifyUserCredentials(createDTO.Auth.Username, createDTO.Auth.Password);
            if (user is null) return Unauthorized();

            Post newPost = new Post
            {
                SubforumId = createDTO.SubforumId,
                Title = createDTO.Title,
                Body = createDTO.Body,
                WrittenByUserId = user.Id,
                PostedDate = DateTime.Now
            };

            await posts.AddAsync(newPost);

            return Created("/posts/" + newPost.Id, ToPostDTO(newPost));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Rediger et opslag via id
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="updateDTO">DTO objekt med ændringer til opslaget og loginoplysninger</param>
    [HttpPost("{id:int}")]
    public async Task<ActionResult> UpdatePost([FromRoute] int id, UpdatePostDTO updateDTO)
    {
        try
        {
            Post post = await posts.GetAsync(id);

            // Tjek login oplysninger
            User? user = await users.VerifyUserCredentials(updateDTO.Auth.Username, updateDTO.Auth.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");

            // Tjek om brugeren har rettighed til at ændre opslaget
            Subforum subforum = await subforums.GetAsync(post.SubforumId);
            if (post.WrittenByUserId != user.Id && subforum.ModeratorUserId != user.Id)
                return Unauthorized("Du har ikke rettighed til at ændre dette opslag");

            post.Title = updateDTO.Title;
            post.Body = updateDTO.Body;

            post.Edited = true;
            post.EditedDate = DateTime.Now;

            await posts.UpdateAsync(post);
            return Ok("Opslaget blev ændret");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Slet et opslag via id
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="auth">DTO med loginoplysninger</param>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost([FromRoute] int id, UserLoginDTO auth)
    {
        try
        {
            Post post = await posts.GetAsync(id);

            // Tjek login oplysninger
            User? user = await users.VerifyUserCredentials(auth.Username, auth.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");

            // Tjek om brugeren har rettighed til at slette opslaget
            Subforum subforum = await subforums.GetAsync(post.SubforumId);
            if (post.WrittenByUserId != user.Id && subforum.ModeratorUserId != user.Id)
                return Unauthorized("Du har ikke rettighed til at slette dette opslag");

            await posts.DeleteAsync(id);
            return Ok("Opslaget blev slettet");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Hent kommentarer under et opslag, eventuelt angiv en maks dybde at hente kommentarers kommentar med depth query-parameter
    /// </summary>
    /// <param name="id"></param>
    /// <param name="depth"></param>
    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult> GetPostComments([FromRoute] int id, [FromQuery] int depth = 3)
    {
        try
        {
            return Ok(await GetCommentDTOs(id, depth));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Like et opslag
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="loginDTO">Login-oplysninger</param>
    [HttpPost("{id:int}/like")]
    public async Task<ActionResult> Like([FromRoute] int id, [FromBody] UserLoginDTO loginDTO)
    {
        try
        {
            User? user = await users.VerifyUserCredentials(loginDTO.Username, loginDTO.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");

            Post post = await posts.GetAsync(id); // Thrower hvis opslaget ikke findes

            await reactions.AddAsync(new Reaction
            {
                Type = "like",
                PostId = id,
                UserId = user.Id
            });

            return Ok("Du har liket opslaget");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Fjern et like fra et opslag
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="loginDTO">Login-oplysninger</param>
    [HttpDelete("{id:int}/like")]
    public async Task<ActionResult> Unlike([FromRoute] int id, [FromBody] UserLoginDTO loginDTO)
    {
        try
        {
            User? user = await users.VerifyUserCredentials(loginDTO.Username, loginDTO.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");

            Post post = await posts.GetAsync(id); // Thrower hvis opslaget ikke findes

            await reactions.DeleteAsync(new Reaction
            {
                Type = "like",
                PostId = id,
                UserId = user.Id
            });

            return Ok("Dit like blev fjernet");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Dislike et opslag
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="loginDTO">Login-oplysninger</param>
    [HttpPost("{id:int}/dislike")]
    public async Task<ActionResult> Dislike([FromRoute] int id, [FromBody] UserLoginDTO loginDTO)
    {
        try
        {
            User? user = await users.VerifyUserCredentials(loginDTO.Username, loginDTO.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");

            Post post = await posts.GetAsync(id); // Thrower hvis opslaget ikke findes

            await reactions.AddAsync(new Reaction
            {
                Type = "dislike",
                PostId = id,
                UserId = user.Id
            });

            return Ok("Du har disliket opslaget");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Fjern et dislike fra et opslag
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="loginDTO">Login-oplysninger</param>
    [HttpDelete("{id:int}/dislike")]
    public async Task<ActionResult> Undislike([FromRoute] int id, [FromBody] UserLoginDTO loginDTO)
    {
        try
        {
            User? user = await users.VerifyUserCredentials(loginDTO.Username, loginDTO.Password);
            if (user is null) return Unauthorized("Ugyldig brugernavn eller password");

            Post post = await posts.GetAsync(id); // Thrower hvis opslaget ikke findes

            await reactions.DeleteAsync(new Reaction
            {
                Type = "dislike",
                PostId = id,
                UserId = user.Id
            });

            return Ok("Dit dislike blev fjernet");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}