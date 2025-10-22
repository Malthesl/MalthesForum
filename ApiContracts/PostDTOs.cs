namespace ApiContracts;

public class PostDTO
{
    public required string Type { get; init; }
    public required int Id { get; init; }
    public string? Title { get; init; }
    public required string Body { get; init; }
    public required UserDTO WrittenBy { get; init; }
    public required int SubforumId { get; init; }
    public int? CommentedOnPostId { get; init; }
    public required int CommentsCount { get; init; }
    public required PostDTO[] Comments { get; init; }
    public required Dictionary<string, int> Reactions { get; init; }
    public required DateTime PostedDate { get; init; }
    public required bool Edited { get; init; }
    public required DateTime EditedDate { get; init; }
}

public class UpdatePostDTO
{
    public required string? Title { get; init; }
    public required string? Body { get; init; }
    public required UserLoginDTO Auth { get; init; }
}

public class CreatePostDTO
{
    public required int SubforumId { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required UserLoginDTO Auth { get; init; }
}

public class CreateCommentDTO
{
    public required string Body { get; init; }
    public required UserLoginDTO Auth { get; init; }
}