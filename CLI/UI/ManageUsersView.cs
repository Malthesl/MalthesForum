using Entities;
using RepositoryContracts;

namespace CLI.UI;

public class ManageUsersView(
    ViewState viewState,
    IUserRepository users
) : View
{
    private readonly Dictionary<string, Command> _commands = new Dictionary<string, Command>
    {
        {
            "list",
            new Command
            {
                Description = "Se alle brugere",
                Syntax = "users list",
                args = 0
            }
        },
        {
            "login",
            new Command
            {
                Description = "Log ind som bruger",
                Syntax = "users login <brugernavn> <adgangskode>",
                args = 2
            }
        },
        {
            "create",
            new Command
            {
                Description = "Opret bruger",
                Syntax = "users create <brugernavn> <adgangskode>",
                args = 2
            }
        },
        {
            "delete",
            new Command
            {
                Description = "Slet bruger",
                Syntax = "users delete <brugernavn>",
                args = 1
            }
        },
        {
            "rename",
            new Command
            {
                Description = "Rediger brugernavn",
                Syntax = "users rename <brugernavn> <nyt brugernavn>",
                args = 2
            }
        },
        {
            "password",
            new Command
            {
                Description = "Rediger adgangskode",
                Syntax = "users password <adgangskode> <ny adgangskode>",
                args = 2
            }
        }
    };
    
    public async Task run(String commandStr)
    {
        string[] args = commandStr.Split(' ');

        _commands.TryGetValue(args[0], out Command? command);
        
        if (command is null)
        {
            Console.WriteLine("Ugyldig kommando!");
            Console.WriteLine("Hjælp til subforum-kommando:");
            foreach (var c in _commands)
            {
                Console.WriteLine($" - {c.Value.Description}: {c.Value.Syntax}");
            }
            return;
        }

        if (args.Length != command.args + 1)
        {
            Console.WriteLine("For få argumenter!");
            Console.WriteLine($"Syntaks: {command.Syntax}");
            return;
        }

        switch (args[0])
        {
            case "login":
                await LoginUser(args[1], args[2]);
                break;
            case "create":
                await CreateUser(args[1], args[2]);
                break;
            case "delete":
                await DeleteUser(args[1]);
                break;
            case "list":
                await ListUsers();
                break;
            case "rename":
                await RenameUser(args[1], args[2]);
                break;
            case "password":
                await ResetUserPassword(args[1], args[2]);
                break;
        }
    }

    private async Task ListUsers()
    {
        List<User> list = users.GetMany().ToList();

        Console.WriteLine($"Der findes {list.Count} brugere");
        foreach (User user in list)
        {
            Console.WriteLine($" - {user.Username} ({user.Id})");
        }
    }

    private async Task LoginUser(string username, string password)
    {
        User? user = await users.GetByUsername(username);

        if (user is null)
        {
            Console.WriteLine($"Brugeren {username} findes ikke");
            return;
        }

        if (user.Password != password)
        {
            Console.WriteLine("Adgangskoden er forkert");
            return;
        }

        viewState.CurrentUser = user.Id;
        viewState.CurrentUsername = user.Username;
        Console.WriteLine("Logget ind som bruger " + username);
    }

    private async Task CreateUser(string username, string password)
    {
        User? user = await users.GetByUsername(username);

        if (user is not null)
        {
            Console.WriteLine("Brugernavnet er allerede taget");
            return;
        }

        user = await users.AddAsync(new User
        {
            Username = username,
            Password = password
        });

        viewState.CurrentUser = user.Id;
        viewState.CurrentUsername = user.Username;
        Console.WriteLine($"Oprettede bruger {username}");
        Console.WriteLine($"Logget ind som bruger {username}");
    }

    private async Task DeleteUser(string username)
    {
        User? user = await users.GetByUsername(username);

        if (user is null)
        {
            Console.WriteLine($"Brugeren ved navn {username} findes ikke");
            return;
        }

        await users.DeleteAsync(user.Id);

        Console.WriteLine($"Slettede bruger {username}");
    }

    private async Task RenameUser(string oldUsername, string newUsername)
    {
        User? newUser = await users.GetByUsername(newUsername);

        if (newUser is not null)
        {
            Console.WriteLine("Brugernavnet er allerede taget");
            return;
        }

        User? user = await users.GetByUsername(oldUsername);

        if (user is null)
        {
            Console.WriteLine($"Brugeren {oldUsername} findes ikke");
            return;
        }

        user.Username = newUsername;

        await users.UpdateAsync(user);

        Console.WriteLine($"Brugeren {oldUsername} hedder nu {newUsername}");
    }

    private async Task ResetUserPassword(string username, string newPassword)
    {
        User? user = await users.GetByUsername(username);

        if (user is null)
        {
            Console.WriteLine($"Brugeren {username} findes ikke");
            return;
        }

        user.Password = newPassword;

        await users.UpdateAsync(user);

        Console.WriteLine($"Skiftede brugeren '{user.Username}' adgangskode");
    }
}