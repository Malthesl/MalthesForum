namespace ApiContracts;

public class PostDTO
{
    public required string Type { get; set; }
    public required int Id { get; set; }
    public string? Title { get; set; }
    public required string Body { get; set; }
    public required UserDTO WrittenBy { get; set; }
    public required int SubforumId { get; set; }
    public required string SubforumUrl { get; set; }
    public int? CommentedOnPostId { get; set; }
    public required int CommentsCount { get; set; }
    public required List<PostDTO> Comments { get; set; }
    public required Dictionary<string, int> Reactions { get; set; }
    public required DateTime PostedDate { get; set; }
    public required bool Edited { get; set; }
    public required DateTime EditedDate { get; set; }
    public required List<string> HasReacted { get; set; }
}

public class UpdatePostDTO
{
    public required string? Title { get; set; }
    public required string? Body { get; set; }
}

public class CreatePostDTO
{
    public required int SubforumId { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
}

public class CreateCommentDTO
{
    public required string Body { get; set; }
}