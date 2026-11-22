using Entities;
using Microsoft.EntityFrameworkCore;

namespace EfcRepositories;

/// <summary>
/// The EF Core "entry point" to the database - one DbSet per entity.
///
/// The connection string is hardcoded here (rather than injected via
/// DbContextOptions) to match how `dotnet ef` tooling is run directly from
/// this project's folder (see the assignment's Part 7 instructions): with
/// a parameterless constructor + OnConfiguring, both `dotnet ef migrations
/// add` and the Web API (via AddDbContext&lt;ForumDbContext&gt;()) can create
/// this context without any extra configuration.
///
/// The relative path ("../EfcRepositories/app.db") is written so the
/// database file always ends up inside this project's own folder,
/// regardless of whether the current directory is this project's folder
/// (running `dotnet ef ...` as instructed) or the WebApi project's folder
/// (running the API itself) - both resolve to the same place.
/// </summary>
public class ForumDbContext : DbContext
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=../EfcRepositories/app.db");
    }
}
