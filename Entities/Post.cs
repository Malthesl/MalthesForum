using System.ComponentModel.DataAnnotations;

namespace Entities;

public class Post
{
    [Key]
    public int Id { get; set; }
    public string? Title { get; set; }
    public required string Body { get; set; }
    public required DateTime PostedDate { get; set; }
    public bool Edited { get; set; }
    public DateTime EditedDate { get; set; }
    public required int WrittenById { get; set; }
    public User WrittenBy { get; set; }
    public required int SubforumId { get; set; }
    public Subforum Subforum { get; set; }
    public int? CommentedOnId { get; set; }
    public Post? CommentedOn { get; set; }
    public List<Reaction> Reactions { get; set; }
}