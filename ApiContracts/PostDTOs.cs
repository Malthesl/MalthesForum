namespace ApiContracts;

public interface IPostDTO
{
    string Type { get; }
    int Id { get; }
    string? Title { get; }
    string Body { get; }
    UserDTO WrittenBy { get; }
    int SubforumId { get; }
    int? CommentedOnPostId { get; }
    int CommentsCount { get; }
    IPostDTO[] Comments { get; }
    Dictionary<string, int> Reactions { get; }
    DateTime PostedDate { get; }
    bool Edited { get; }
    DateTime EditedDate { get; }
}

public class PostDTO : IPostDTO
{
    public string Type => "post";
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required UserDTO WrittenBy { get; init; }
    public required int SubforumId { get; init; }
    public int? CommentedOnPostId => null;
    public required int CommentsCount { get; init; }
    public required IPostDTO[] Comments { get; init; }
    public required Dictionary<string, int> Reactions { get; init; }
    public required DateTime PostedDate { get; init; }
    public required bool Edited { get; init; }
    public required DateTime EditedDate { get; init; }
}

public class CommentDTO : IPostDTO
{
    public string Type => "comment";
    public required int Id { get; init; }
    public string? Title => null;
    public required string Body { get; init; }
    public required UserDTO WrittenBy { get; init; }
    public required int SubforumId { get; init; }
    public required int? CommentedOnPostId { get; init; }
    public required int CommentsCount { get; init; }
    public required IPostDTO[] Comments { get; init; }
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