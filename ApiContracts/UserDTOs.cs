namespace ApiContracts;

public class UserDTO
{
    public required int Id { get; set; }
    public required string Username { get; set; }
}

public class CreateUserDTO
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class UpdateUserDTO
{
    public required string? Username { get; set; }
    public required string? Password { get; set; }
}

public class UserLoginDTO
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}