using RepositoryContracts;

namespace CLI.UI;

public class CliApp(
    IUserRepository userRepository,
    ISubforumRepository subforumRepository,
    IPostRepository postRepository,
    IReactionRepository reactionRepository,
    ViewState viewState
)
{
    private readonly ManageUsersView _manageUsers = new(viewState, userRepository);

    private readonly ManagePostsView _managePosts =
        new(viewState, postRepository, subforumRepository, userRepository, reactionRepository);

    private readonly ManageSubforumsView _manageSubforums = new(viewState, subforumRepository);

    public async Task startAsync()
    {
        Console.WriteLine("Velkommen til Malthes Forum CLI");
        Console.WriteLine("Skriv 'users' for at administrere brugere");
        Console.WriteLine("Skriv 'post' for at administrere posts");
        Console.WriteLine("Skriv 'subforum' for at administrere subforummer");
        Console.WriteLine("Der er ingen permissions/authentication, men nogle funktioner kræver at være logget ind");
        Console.WriteLine("Skriv 'users login <brugernavn> <adgangskode>' for at logge ind");
        Console.WriteLine("Skriv 'users create <brugernavn> <adgangskode>' for at oprette en bruge");
        
        while (true)
        {
            try
            {
                Console.Write($"{viewState.CurrentUsername}>");
                string? input = Console.ReadLine();
                if (input is null) continue;

                var command = input.Split(' ')[0];
                var rest = input.Substring(command.Length).TrimStart();

                switch (command)
                {
                    case "users":
                        await _manageUsers.run(rest);
                        break;
                    case "post":
                        await _managePosts.run(rest);
                        break;
                    case "subforum":
                        await _manageSubforums.run(rest);
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    default:
                        Console.WriteLine($"Kommando {command} findes ikke");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("### Der skete en fejl ###");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}