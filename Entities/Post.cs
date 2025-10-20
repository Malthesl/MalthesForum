namespace Entities;

public class Post
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public required string Body { get; set; }
    public required DateTime PostedDate { get; set; }
    public bool Edited { get; set; }
    public DateTime EditedDate { get; set; }
    public required int WrittenByUserId { get; set; }
    public required int SubforumId { get; set; }
    public int? CommentedOnPostId { get; set; }
}