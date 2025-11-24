using Entities;
using RepositoryContracts;

namespace CLI.UI;

public class ManageSubforumsView(
    ViewState viewState,
    ISubforumRepository subforums
) : View
{
    private readonly Dictionary<string, Command> _commands = new Dictionary<string, Command>
    {
        {
            "list",
            new Command
            {
                Description = "Liste over subforums",
                Syntax = "subforum list",
                args = 0
            }
        },
        {
            "create",
            new Command
            {
                Description = "Liste over subforums",
                Syntax = "subforum create <name>",
                args = 1
            }
        },
        {
            "delete",
            new Command
            {
                Description = "Slet et subforum",
                Syntax = "subforum delete <name>",
                args = 1
            }
        },
        {
            "rename",
            new Command
            {
                Description = "Omdøb et subforum",
                Syntax = "subforum rename <old name> <new name>",
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
            case "list":
                await ListSubforums();
                break;
            case "create":
                await CreateSubforum(args[1]);
                break;
            case "delete":
                await DeleteSubforum(args[1]);
                break;
            case "rename":
                await RenameSubforum(args[1], args[2]);
                break;
        }
    }

    private async Task ListSubforums()
    {
        List<Subforum> list = subforums.GetMany().ToList();
        Console.WriteLine($"Der findes {list.Count} subforummer");
        foreach (Subforum subforum in list)
        {
            Console.WriteLine($"- {subforum.Name} ({subforum.Id})");
        }
    }


    private async Task CreateSubforum(string subforumName)
    {
        if (viewState.CurrentUser is null)
        {
            Console.WriteLine($"Du er ikke logget ind!");
            return;
        }
        
        Subforum? subforum = await subforums.GetByName(subforumName);

        if (subforum is not null)
        {
            Console.WriteLine("Navnet er allerede taget");
            return;
        }

        await subforums.AddAsync(new Subforum
        {
            Name = subforumName,
            Url = subforumName,
            ModeratorId = (int)viewState.CurrentUser
        });

        Console.WriteLine($"Oprettede subforum {subforumName}");
    }

    private async Task DeleteSubforum(string subforumName)
    {
        Subforum? subforum = await subforums.GetByName(subforumName);

        if (subforum is null)
        {
            Console.WriteLine($"Subforum ved navn {subforumName} findes ikke");
            return;
        }

        await subforums.DeleteAsync(subforum.Id);

        Console.WriteLine($"Slettede subforum {subforumName}");
    }

    private async Task RenameSubforum(string oldName, string newName)
    {
        Subforum? newSubforum = await subforums.GetByName(newName);

        if (newSubforum is not null)
        {
            Console.WriteLine("Navnet er allerede taget");
            return;
        }

        Subforum? subforum = await subforums.GetByName(oldName);

        if (subforum is null)
        {
            Console.WriteLine($"Subforum ved navn {oldName} findes ikke");
            return;
        }

        subforum.Name = newName;

        await subforums.UpdateAsync(subforum);

        Console.WriteLine($"Brugeren {oldName} hedder nu {newName}");
    }
}