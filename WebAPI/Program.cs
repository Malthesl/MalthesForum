using FileRepositories;
using RepositoryContracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IPostRepository, PostFileRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionFileRepository>();
builder.Services.AddScoped<ISubforumRepository, SubforumFileRepository>();
builder.Services.AddScoped<IUserRepository, UserFileRepository>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();