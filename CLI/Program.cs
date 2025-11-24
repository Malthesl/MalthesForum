using CLI.UI;
using EfcRepositories;
using RepositoryContracts;
using AppContext = EfcRepositories.AppContext;

namespace CLI;

class Program
{
    static async Task Main(string[] args)
    {
        AppContext ctx = new AppContext(); // ikke testet, aner ikke om det virker, men altså 
        
        IUserRepository userRepository = new UserRepository(ctx);
        ISubforumRepository subforumRepository = new SubforumRepository(ctx);
        IPostRepository postRepository = new PostRepository(ctx);
        IReactionRepository reactionRepository = new ReactionRepository(ctx);

        CliApp cliApp = new CliApp(userRepository, subforumRepository, postRepository, reactionRepository, new ViewState());
        await cliApp.startAsync();
    }
}