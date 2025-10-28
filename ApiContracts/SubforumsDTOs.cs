namespace ApiContracts;

public class SubforumDTO
{
    public required int Id { get; set; }
    public required string Name { get; set; } 
    public required string URL { get; set; }
    public required UserDTO Moderator { get; set; }
    public required int PostsCount { get; set; }
}

public class CreateSubforumDTO
{
    public required string Name { get; set; }
    public required string URL { get; set; }
}

public class UpdateSubforumDTO
{
    public required string? Name { get; set; }
    public required string? URL { get; set; }
    public required int? ModeratorId { get; set; }
}