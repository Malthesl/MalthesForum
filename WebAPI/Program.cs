using System.Text;
using FileRepositories;
using RepositoryContracts;
using WebAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "MalthesForum",
            ValidAudience = "MalthesUsers",
            IssuerSigningKey = new SymmetricSecurityKey("SuperSecretKeyThatIsAtMinimum32CharactersLong"u8.ToArray())
        };
    });

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
builder.Services.AddScoped<IPostRepository, PostFileRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionFileRepository>();
builder.Services.AddScoped<ISubforumRepository, SubforumFileRepository>();
builder.Services.AddScoped<IUserRepository, UserFileRepository>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();