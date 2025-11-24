using System.ComponentModel.DataAnnotations;

namespace Entities;

public class User
{
    [Key]
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public List<Post> Posts { get; set; }
    public List<Reaction> Reactions { get; set; }
}