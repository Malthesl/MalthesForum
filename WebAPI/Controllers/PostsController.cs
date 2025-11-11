using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Authorization;
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
    private async Task<PostDTO> ToPostDTO(Post post, int? userId, int commentDepth = 0)
    {
        User writtenBy = await users.GetAsync(post.WrittenByUserId);

        return new PostDTO
        {
            Type = "post",
            Id = post.Id,
            Body = post.Body,
            Title = post.Title,
            SubforumId = post.SubforumId,
            SubforumUrl = (await subforums.GetAsync(post.SubforumId)).URL,
            PostedDate = post.PostedDate,
            Edited = post.Edited,
            EditedDate = post.EditedDate,
            WrittenBy = new UserDTO
            {
                Id = writtenBy.Id,
                Username = writtenBy.Username
            },
            CommentsCount = await posts.GetTotalComments(post.Id),
            Comments = await GetCommentDTOs(post.Id, commentDepth, userId),
            Reactions = await reactions.GetTotalOfEachTypeAsync(post.Id),
            HasReacted = userId is null ? [] : await reactions.GetReactionsOnPostByUser(post.Id, userId)
        };
    }

    /// <summary>
    /// Hjælpemetode der opretter DTO fra en kommentar
    /// </summary>
    private async Task<PostDTO> ToCommentDTO(Post comment, int? userId, int commentDepth = 0)
    {
        User writtenBy = await users.GetAsync(comment.WrittenByUserId);

        return new PostDTO
        {
            Type = "comment",
            Id = comment.Id,
            Body = comment.Body,
            WrittenBy = new UserDTO
            {
                Id = comment.WrittenByUserId,
                Username = writtenBy.Username
            },
            SubforumId = comment.SubforumId,
            SubforumUrl = (await subforums.GetAsync(comment.SubforumId)).URL,
            CommentedOnPostId = comment.CommentedOnPostId ?? -1,
            CommentsCount = await posts.GetTotalComments(comment.Id),
            Comments = await GetCommentDTOs(comment.Id, commentDepth - 1, userId),
            Reactions = await reactions.GetTotalOfEachTypeAsync(comment.Id),
            PostedDate = comment.PostedDate,
            Edited = comment.Edited,
            EditedDate = comment.EditedDate,
            HasReacted = userId is null ? [] : await reactions.GetReactionsOnPostByUser(comment.Id, userId)
        };
    }

    /// <summary>
    /// Hjælpemetode til at hente kommentarer under et opslag
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="depth">Dybde at hente kommentarer</param>
    /// <param name="userId">Hent opslagene som en bruger (inkluder informationer om brugeren har reageret på opslaget)</param>
    private async Task<List<PostDTO>> GetCommentDTOs(int id, int depth, int? userId)
    {
        if (depth == 0) return [];

        List<Post> comments = await posts.GetComments(id);


        return (await Task.WhenAll(comments.OrderBy(c => c.PostedDate).Reverse()
            .Select(comment => ToCommentDTO(comment, userId, depth - 1)))).ToList();
    }

    /// <summary>
    /// Query efter en masse forskellige parametre.
    /// </summary>
    /// <param name="subforumId">Kun inkluder opslag fra et bestemt subforum</param>
    /// <param name="subforumUrl">Kun inkluder opslag fra et bestemt subforum</param>
    /// <param name="type">Brug 'post' til kun at inkluder opslag eller 'comment' til kun at inkluder kommentarer</param>
    /// <param name="userId">Kun inkluder opslag skrevet af en bestemt bruger</param>
    /// <param name="search">Kun inkluder opslag der indeholder strengen i enten titel eller body</param>
    /// <param name="before">Kun inkluder opslag fra før en dato</param>
    /// <param name="after">Kun inkluder opslag fra efter en dato</param>
    /// <param name="asUserId">Brugeren som henter opslagene (for at vise hvilke reaktioner brugeren har givet)</param>
    /// <param name="commentDepth">Angiv en dybde at returner kommentarer under opslag, f.eks. vil en dybde på 2 returner kommentarer på alle resultater + kommentarerene på de første kommentarer</param>
    /// <param name="limit">Angiv et maks mængde resultater</param>
    /// <param name="offset">Offset indeks for det første resultat</param>
    [HttpGet]
    public async Task<ActionResult> GetPosts(
        [FromQuery] int? subforumId, [FromQuery] string? subforumUrl, [FromQuery] string? type, [FromQuery] int? userId,
        [FromQuery] string? search, [FromQuery] DateTime? before, [FromQuery] DateTime? after,
        [FromQuery] int commentDepth = 0, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        string? userIdClaim = User.FindFirst("Id")?.Value;
        int? asUserId = userIdClaim is not null ? int.Parse(userIdClaim) : null;

        IQueryable<Post> query = posts.GetMany().OrderBy(p => p.PostedDate).Reverse();

        // Query efter de forskellige parametre
        if (subforumUrl is not null) subforumId = (await subforums.GetByURL(subforumUrl))!.Id;
        if (subforumId is not null) query = query.Where(p => p.SubforumId == subforumId);
        if (type is not null) query = query.Where(p => (p.CommentedOnPostId == null) == (type == "post"));
        if (userId is not null) query = query.Where(p => p.WrittenByUserId == userId);
        if (search is not null)
            query = query.Where(p =>
                p.Title != null && p.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                p.Body.Contains(search, StringComparison.CurrentCultureIgnoreCase));
        if (before is not null) query = query.Where(p => p.PostedDate < before);
        if (after is not null) query = query.Where(p => p.PostedDate > after);

        int count = query.Count();

        // Kun vælg resultater efter offset og maks limit
        query = query.Skip(offset).Take(limit);

        // Opret DTOs fra resultaterne
        PostDTO[] dtos = await Task.WhenAll(
            query.AsEnumerable().Select(post =>
                post.CommentedOnPostId is null
                    ? ToPostDTO(post, asUserId, commentDepth)
                    : ToCommentDTO(post, asUserId, commentDepth))
        );

        return Ok(new QueryResponseDTO<Object>
        {
            TotalResults = count,
            StartIndex = offset,
            EndIndex = Math.Min(offset + limit, count),
            Results = dtos
        });
    }

    /// <summary>
    /// Hent et opslag via id
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="commentDepth"></param>
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetPost([FromRoute] int id, [FromQuery] int commentDepth = 3)
    {
        Post post = await posts.GetAsync(id);

        string? userIdClaim = User.FindFirst("Id")?.Value;
        int? userId = userIdClaim is not null ? int.Parse(userIdClaim) : null;

        return Ok(
            post.CommentedOnPostId is null
                ? await ToPostDTO(post, userId, commentDepth)
                : await ToCommentDTO(post, userId, commentDepth)
        );
    }

    ///
    /// Opret et opslag i et subforum
    ///
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreatePost([FromBody] CreatePostDTO createDTO)
    {
        string userIdClaim = User.FindFirst("Id")!.Value;
        int userId = int.Parse(userIdClaim);

        Post newPost = new Post
        {
            SubforumId = createDTO.SubforumId,
            Title = createDTO.Title,
            Body = createDTO.Body,
            WrittenByUserId = userId,
            PostedDate = DateTime.Now
        };

        await posts.AddAsync(newPost);

        return Created("/posts/" + newPost.Id, await ToPostDTO(newPost, userId));
    }

    /// <summary>
    /// Rediger et opslag via id
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="updateDTO">DTO objekt med ændringer til opslaget og loginoplysninger</param>
    [HttpPost("{id:int}")]
    [Authorize]
    public async Task<ActionResult> UpdatePost([FromRoute] int id, UpdatePostDTO updateDTO)
    {
        Post post = await posts.GetAsync(id);

        string userIdClaim = User.FindFirst("Id")!.Value;
        int userId = int.Parse(userIdClaim);

        // Tjek om brugeren har rettighed til at ændre opslaget
        Subforum subforum = await subforums.GetAsync(post.SubforumId);
        if (post.WrittenByUserId != userId && subforum.ModeratorUserId != userId)
            return Unauthorized("Du har ikke rettighed til at ændre dette opslag");

        if (updateDTO.Title is not null) post.Title = updateDTO.Title;
        if (updateDTO.Body is not null) post.Body = updateDTO.Body;

        post.Edited = true;
        post.EditedDate = DateTime.Now;

        await posts.UpdateAsync(post);
        return Ok(new SuccessDTO("Opslaget blev ændret"));
    }

    /// <summary>
    /// Slet et opslag via id
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="auth">DTO med loginoplysninger</param>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> DeletePost([FromRoute] int id)
    {
        Post post = await posts.GetAsync(id);

        string userIdClaim = User.FindFirst("Id")!.Value;
        int userId = int.Parse(userIdClaim);

        // Tjek om brugeren har rettighed til at slette opslaget
        Subforum subforum = await subforums.GetAsync(post.SubforumId);
        if (post.WrittenByUserId != userId && subforum.ModeratorUserId != userId)
            return Unauthorized("Du har ikke rettighed til at slette dette opslag");

        await posts.DeleteAsync(id);
        return Ok(new SuccessDTO("Opslaget blev slettet"));
    }

    /// <summary>
    /// Hent kommentarer under et opslag, eventuelt angiv en maks dybde at hente kommentarers kommentar med depth query-parameter
    /// </summary>
    /// <param name="id"></param>
    /// <param name="depth"></param>
    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult> GetPostComments([FromRoute] int id, [FromQuery] int depth = 3)
    {
        return Ok(await GetCommentDTOs(id, depth, null));
    }

    ///
    /// Opret en kommentar på et opslag
    ///
    [HttpPost("{id:int}/comment")]
    [Authorize]
    public async Task<ActionResult> CreateComment([FromRoute] int id, [FromBody] CreateCommentDTO createDTO)
    {
        // Verificer brugeren og hent bruger id
        string userIdClaim = User.FindFirst("Id")!.Value;
        int userId = int.Parse(userIdClaim);

        Post parentPost = await posts.GetAsync(id);

        Post newPost = new Post
        {
            SubforumId = parentPost.SubforumId,
            Body = createDTO.Body,
            WrittenByUserId = userId,
            PostedDate = DateTime.Now,
            CommentedOnPostId = id
        };

        await posts.AddAsync(newPost);

        return Created("/posts/" + newPost.Id, await ToCommentDTO(newPost, userId));
    }

    /// <summary>
    /// Reager på et opslag
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="type">Typen af reaktion (like, dislike)</param>
    [HttpPost("{id:int}/react")]
    [Authorize]
    public async Task<ActionResult> React([FromRoute] int id, [FromQuery] string type)
    {
        string userIdClaim = User.FindFirst("Id")!.Value;
        int userId = int.Parse(userIdClaim);

        Post post = await posts.GetAsync(id); // Thrower hvis opslaget ikke findes

        await reactions.AddAsync(new Reaction
        {
            Type = type.ToLower(),
            PostId = id,
            UserId = userId
        });

        return Ok(new SuccessDTO($"Du reagerede med '{type}' på opslaget"));
    }

    /// <summary>
    /// Fjern en reaktion fra et opslag
    /// </summary>
    /// <param name="id">ID'et på opslaget</param>
    /// <param name="type">Typen af reaktion (like, dislike)</param>
    [HttpDelete("{id:int}/react")]
    [Authorize]
    public async Task<ActionResult> Unreact([FromRoute] int id, [FromQuery] string type)
    {
        string userIdClaim = User.FindFirst("Id")!.Value;
        int userId = int.Parse(userIdClaim);

        Post post = await posts.GetAsync(id); // Thrower hvis opslaget ikke findes

        await reactions.DeleteAsync(new Reaction
        {
            Type = type,
            PostId = id,
            UserId = userId
        });

        return Ok(new SuccessDTO($"Du fjernede din reaction med '{type}' fra opslaget"));
    }
}