namespace ApiContracts;

public class UserDTO
{
    public required int Id { get; init; }
    public required string Username { get; init; }
}

public class CreateUserDTO
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public class UpdateUserDTO
{
    public required string? Username { get; init; }
    public required string? Password { get; init; }
    public required UserLoginDTO Auth { get; init; }
}

public class UserAuthDTO
{
    public required UserLoginDTO Auth { get; init; }
}

public class UserLoginDTO
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}