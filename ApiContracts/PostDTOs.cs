namespace ApiContracts;

public class PostDTO
{
    public string Type => "post"; 
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required UserDTO WrittenBy { get; init; }
    public required int SubforumId { get; init; }
    public required int CommentsCount { get; init; }
    public required CommentDTO[] Comments { get; init; }
    public required int Likes { get; init; }
    public required int Dislikes { get; init; }
    public required DateTime PostedDate { get; init; }
    public required bool Edited { get; init; }
    public required DateTime EditedDate { get; init; }
}

public class CommentDTO
{
    public string Type => "comment";

    public required int Id { get; init; }
    public required string Body { get; init; }
    public required UserDTO WrittenBy { get; init; }
    public required int SubforumId { get; init; }
    public required int CommentedOnPostId { get; init; }
    public required int CommentsCount { get; init; }
    public required CommentDTO[] Comments { get; init; }
    public required int Likes { get; init; }
    public required int Dislikes { get; init; }
    public required DateTime PostedDate { get; init; }
    public required bool Edited { get; init; }
    public required DateTime EditedDate { get; init; }
}

public class UpdatePostDTO
{
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required UserLoginDTO Auth { get; init; }
}

public class CreatePostDTO
{
    public required int SubforumId { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required UserLoginDTO Auth { get; init; }
}