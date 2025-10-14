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
    public required string Name { get; init; }
    public required string URL { get; init; }
    public required UserLoginDTO Auth { get; init; }
}

public class UpdateSubforumDTO
{
    public required string Name { get; init; }
    public required string URL { get; init; }
    public required int ModeratorId { get; init; }
    public required UserLoginDTO Auth { get; init; }
}