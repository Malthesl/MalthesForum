namespace Entities;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public DateTime PostedDate { get; set; }
    public bool Edited { get; set; }
    public DateTime EditedDate { get; set; }
    public int WrittenByUserId { get; set; }
    public int SubforumId { get; set; }
    public int? CommentedOnPostId { get; set; }
}