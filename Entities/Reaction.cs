using System.ComponentModel.DataAnnotations;

namespace Entities;

public class Reaction
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PostId { get; set; }
    public User User { get; set; }
    public Post Post { get; set; }
    public required string Type { get; set; } // f.eks. like, dislike
}