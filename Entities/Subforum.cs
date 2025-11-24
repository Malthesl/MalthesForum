using System.ComponentModel.DataAnnotations;

namespace Entities;

public class Subforum
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; } 
    public required string Url { get; set; }
    public required int ModeratorId { get; set; }
    public User Moderator { get; set; }
    public List<Post> Posts { get; set; }
}