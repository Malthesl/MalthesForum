namespace Entities;

public class Reaction
{
    public int UserId { get; set; }
    public int PostId { get; set; }
    public string Type { get; set; } // f.eks. like, dislike
}