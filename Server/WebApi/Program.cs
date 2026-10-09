using FileRepositories;
using RepositoryContracts;
using WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register the repository implementations for Dependency Injection.
// This is the only place the concrete (file-based) repository types are
// known about - controllers only ever ask for the interfaces. Swapping to
// a different implementation later (e.g. a database) means changing only
// these three lines.
builder.Services.AddSingleton<IUserRepository, UserFileRepository>();
builder.Services.AddSingleton<IPostRepository, PostFileRepository>();
builder.Services.AddSingleton<ICommentRepository, CommentFileRepository>();

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
