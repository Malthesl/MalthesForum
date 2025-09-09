using Entities;
using RepositoryContracts;

namespace CLI.UI;

public class ManagePostsView(
    ViewState viewState,
    IPostRepository posts,
    ISubforumRepository subforums,
    IUserRepository users,
    IReactionRepository reactions
) : View
{
    private readonly Dictionary<string, Command> _commands = new Dictionary<string, Command>
    {
        {
            "list",
            new Command
            {
                Description = "Se alle opslag i et subforum",
                Syntax = "post list <subforum>",
                args = 1
            }
        },
        {
            "read",
            new Command
            {
                Description = "Læs et opslag",
                Syntax = "post read <postId>",
                args = 1
            }
        },
        {
            "comments",
            new Command
            {
                Description = "Læs alle kommentar på et opslag",
                Syntax = "post comments <postId>",
                args = 1
            }
        },
        {
            "create",
            new Command
            {
                Description = "Opret et opslag",
                Syntax = "post create <subforum>",
                args = 1
            }
        },
        {
            "like",
            new Command
            {
                Description = "Like et opslag",
                Syntax = "post like <postId>",
                args = 1
            }
        },
        {
            "dislike",
            new Command
            {
                Description = "Dislike et opslag",
                Syntax = "post dislike <postId>",
                args = 1
            }
        },
        {
            "unlike",
            new Command
            {
                Description = "Fjern et like fra et opslag",
                Syntax = "post unlike <postId>",
                args = 1
            }
        },
        {
            "undislike",
            new Command
            {
                Description = "Fjern et dislike fra et opslag",
                Syntax = "post undislike <postId>",
                args = 1
            }
        },
        {
            "comment",
            new Command
            {
                Description = "Skriv en kommentar på et opslag",
                Syntax = "post comment <postId>",
                args = 1
            }
        },
        {
            "delete",
            new Command
            {
                Description = "Slet et opslag",
                Syntax = "post delete <postId>",
                args = 1
            }
        },
        {
            "move",
            new Command
            {
                Description = "Flyt et opslag",
                Syntax = "post move <postId> <subforum>",
                args = 2
            }
        },
        {
            "rename",
            new Command
            {
                Description = "Omdøb et opslag",
                Syntax = "post rename <postId>",
                args = 1
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
                await ListPosts(args[1]);
                break;
            case "read":
                await ReadPost(int.Parse(args[1]));
                break;
            case "comments":
                await ListComments(int.Parse(args[1]));
                break;
            case "create":
                await CreatePost(args[1]);
                break;
            case "comment":
                await CommentPost(int.Parse(args[1]));
                break;
            case "like":
                await LikePost(int.Parse(args[1]));
                break;
            case "dislike":
                await DislikePost(int.Parse(args[1]));
                break;
            case "unlike":
                await UnlikePost(int.Parse(args[1]));
                break;
            case "undislike":
                await UndislikePost(int.Parse(args[1]));
                break;
            case "delete":
                await DeletePost(int.Parse(args[1]));
                break;
            case "move":
                await MovePost(int.Parse(args[1]), args[2]);
                break;
            case "rename":
                await RenamePost(int.Parse(args[1]));
                break;
        }
    }

    private async Task ListPosts(string subforumName)
    {
        Subforum? subforum = await subforums.GetByName(subforumName);

        if (subforum == null)
        {
            Console.WriteLine($"Subforum med navn {subforumName} findes ikke");
            return;
        }

        List<Post> list = await posts.GetPosts(subforum.Id);

        Console.WriteLine($"Der findes {list.Count} opslag i {subforum.Name}");
        foreach (Post post in list)
        {
            User user = await users.GetAsync(post.WrittenByUserId);
            Console.WriteLine(
                $" - {post.Title} af {(await users.GetAsync(post.WrittenByUserId)).Username} ({user.Id}), {(await posts.GetComments(post.Id)).Count} kommentar(er), {await reactions.GetTotalOfTypeAsync(post.Id, "like")} like(s), {await reactions.GetTotalOfTypeAsync(post.Id, "dislike")} dislike(s)");
        }

        Console.WriteLine("Opslag id'er er i parenteser, læs et opslag med `post read <id>`");
    }

    private async Task ReadPost(int postId)
    {
        Post post = await posts.GetAsync(postId);

        Subforum subforum = await subforums.GetAsync(post.SubforumId);
        User user = await users.GetAsync(post.WrittenByUserId);

        List<Post> comments = await posts.GetComments(post.Id);

        int likes = await reactions.GetTotalOfTypeAsync(post.Id, "like");
        int dislikes = await reactions.GetTotalOfTypeAsync(post.Id, "dislike");

        Console.WriteLine($"# {post.Title}");
        Console.WriteLine($"I {subforum.Name} af {user.Username}");
        Console.WriteLine(post.Body);
        Console.WriteLine($"Kommentarer: {comments.Count}, læs med `post comments {post.Id}`");
        Console.WriteLine($"Likes: {likes}, like med `post like {post.Id}`");
        Console.WriteLine($"Dislikes: {dislikes}, dislike med `post dislike {post.Id}`");
    }

    private async Task ListComments(int postId)
    {
        Post post = await posts.GetAsync(postId);

        Subforum subforum = await subforums.GetAsync(post.SubforumId);
        User user = await users.GetAsync(post.WrittenByUserId);

        List<Post> comments = await posts.GetComments(post.Id);

        Console.WriteLine($"# {post.Title}");
        Console.WriteLine($"I {subforum.Name} af {user.Username}");
        Console.WriteLine($"Kommentarer: {comments.Count}");

        foreach (Post comment in comments)
        {
            User commenter = await users.GetAsync(comment.WrittenByUserId);
            Console.WriteLine(
                $" - {comment.Body}\n   af {user.Username} ({commenter.Id}), {(await posts.GetComments(comment.Id)).Count} kommentar(er), {await reactions.GetTotalOfTypeAsync(comment.Id, "like")} like(s), {await reactions.GetTotalOfTypeAsync(comment.Id, "dislike")} dislike(s)");
        }
    }

    private async Task CreatePost(string subforumName)
    {
        if (viewState.CurrentUser is null)
        {
            Console.WriteLine($"Du er ikke logget ind!");
            return;
        }
        
        Subforum? subforum = await subforums.GetByName(subforumName);

        if (subforum == null)
        {
            Console.WriteLine($"Subforum med navn {subforumName} findes ikke");
            return;
        }

        Console.Write("Skriv titel: ");
        string title = Console.ReadLine() ?? "";
        string body = "";
        Console.WriteLine("Skriv dit opslag, skriv 'exit' for at afslutte: ");
        while (true)
        {
            string line = Console.ReadLine() ?? "";
            if (line == "exit") break;
            if (body != "") body += "\n";
            body += line;
        }

        await posts.AddAsync(new Post
        {
            Title = title,
            Body = body,
            SubforumId = subforum.Id,
            WrittenByUserId = (int)viewState.CurrentUser
        });

        Console.WriteLine($"Oprettede dit opslag {title}");
    }

    private async Task CommentPost(int postId)
    {
        if (viewState.CurrentUser is null)
        {
            Console.WriteLine($"Du er ikke logget ind!");
            return;
        }
        
        Post post = await posts.GetAsync(postId);

        string body = "";
        Console.WriteLine("Skriv din kommentar, skriv 'exit' for at afslutte: ");
        while (true)
        {
            string line = Console.ReadLine() ?? "";
            if (line == "exit") break;
            if (body != "") body += "\n";
            body += line;
        }
        
        await posts.AddAsync(new Post
        {
            Body = body,
            SubforumId = post.SubforumId,
            WrittenByUserId = (int)viewState.CurrentUser,
            CommentedOnPostId = post.Id
        });

        Console.WriteLine($"Oprettede din kommentar");
    }

    private async Task LikePost(int postId)
    {
        if (viewState.CurrentUser is null)
        {
            Console.WriteLine($"Du er ikke logget ind!");
            return;
        }
        
        Post post = await posts.GetAsync(postId);

        await reactions.AddAsync(new Reaction()
        {
            PostId = post.Id,
            UserId = (int)viewState.CurrentUser,
            Type = "like"
        });
        
        Console.WriteLine($"Likede {post.Title}");
    }

    private async Task DislikePost(int postId)
    {
        if (viewState.CurrentUser is null)
        {
            Console.WriteLine($"Du er ikke logget ind!");
            return;
        }
        
        Post post = await posts.GetAsync(postId);

        await reactions.AddAsync(new Reaction()
        {
            PostId = post.Id,
            UserId = (int)viewState.CurrentUser,
            Type = "dislike"
        });
        
        Console.WriteLine($"Dislikede {post.Title}");
    }

    private async Task UnlikePost(int postId)
    {
        if (viewState.CurrentUser is null)
        {
            Console.WriteLine($"Du er ikke logget ind!");
            return;
        }
        
        Post post = await posts.GetAsync(postId);

        await reactions.DeleteAsync(new Reaction()
        {
            PostId = post.Id,
            UserId = (int)viewState.CurrentUser,
            Type = "like"
        });
        
        Console.WriteLine($"Fjernede dit like på {post.Title}");
    }

    private async Task UndislikePost(int postId)
    {
        if (viewState.CurrentUser is null)
        {
            Console.WriteLine($"Du er ikke logget ind!");
            return;
        }
        
        Post post = await posts.GetAsync(postId);

        await reactions.DeleteAsync(new Reaction()
        {
            PostId = post.Id,
            UserId = (int)viewState.CurrentUser,
            Type = "dislike"
        });
        
        Console.WriteLine($"Fjernede dit dislike på {post.Title}");
    }

    private async Task DeletePost(int postId)
    {
        Post post = await posts.GetAsync(postId);

        await posts.DeleteAsync(post.Id);
        
        Console.WriteLine($"Fjernede opslaget {post.Title}");
    }

    private async Task MovePost(int postId, string subforumName)
    {
        Post post = await posts.GetAsync(postId);

        Subforum? subforum = await subforums.GetByName(subforumName);

        if (subforum is null)
        {
            Console.WriteLine($"Subforummet findes ikke");
            return;
        }
        
        post.SubforumId = subforum.Id;
        await posts.UpdateAsync(post);
        
        Console.WriteLine($"Postet blev flyttet til subforummet {subforum.Name}");
    }

    private async Task RenamePost(int postId)
    {
        Post post = await posts.GetAsync(postId);

        Console.Write("Skriv en ny titel: ");
        string title = Console.ReadLine() ?? "";
        
        post.Title = title;
        await posts.UpdateAsync(post);
        
        Console.WriteLine("Titlen blev opdateret");
    }
}