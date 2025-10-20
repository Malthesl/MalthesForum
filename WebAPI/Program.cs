using FileRepositories;
using RepositoryContracts;
using WebAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
builder.Services.AddScoped<IPostRepository, PostFileRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionFileRepository>();
builder.Services.AddScoped<ISubforumRepository, SubforumFileRepository>();
builder.Services.AddScoped<IUserRepository, UserFileRepository>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();