using CLI.UI;
using FileRepositories;
using RepositoryContracts;

namespace CLI;

class Program
{
    static async Task Main(string[] args)
    {
        IUserRepository userRepository = new UserFileRepository();
        ISubforumRepository subforumRepository = new SubforumFileRepository();
        IPostRepository postRepository = new PostFileRepository();
        IReactionRepository reactionRepository = new ReactionFileRepository();

        CliApp cliApp = new CliApp(userRepository, subforumRepository, postRepository, reactionRepository, new ViewState());
        await cliApp.startAsync();
    }
}