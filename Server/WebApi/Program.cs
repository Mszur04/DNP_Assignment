using EfcRepositories;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Part 7: swapped the file-based repositories for EF Core ones, backed by
// a real SQLite database. ForumDbContext configures its own connection string
// in OnConfiguring, so no options callback is needed here.
builder.Services.AddDbContext<ForumDbContext>();

// The repositories now depend on ForumDbContext, which is registered above
// with its default (Scoped) lifetime. The repositories must therefore be
// Scoped too, not Singleton like the old file repositories were - a
// Singleton isn't allowed to depend on a Scoped service, and ASP.NET Core
// will refuse to start up if it is.
builder.Services.AddScoped<IUserRepository, EfcUserRepository>();
builder.Services.AddScoped<IPostRepository, EfcPostRepository>();
builder.Services.AddScoped<ICommentRepository, EfcCommentRepository>();

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

// The Blazor app (Part 5) calls this API over HTTP. Blazor Server makes
// those calls from its own server process, not from the browser, so CORS
// isn't strictly required for that - but it's harmless and convenient to
// allow any origin here for local development/testing.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
