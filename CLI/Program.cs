using CLI.UI;
using InMemoryRepositories;
using RepositoryContracts;

namespace CLI;

class Program
{
    static async Task Main(string[] args)
    {
        IUserRepository userRepository = new UserInMemoryRepository();
        ISubforumRepository subforumRepository = new SubforumInMemoryRepository();
        IPostRepository postRepository = new PostInMemoryRepository();
        IReactionRepository reactionRepository = new ReactionInMemoryRepository();

        CliApp cliApp = new CliApp(userRepository, subforumRepository, postRepository, reactionRepository, new ViewState());
        await cliApp.startAsync();
    }
}