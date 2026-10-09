namespace RepositoryContracts;

/// <summary>
/// Thrown by a repository implementation when an entity with a given Id
/// could not be found (GetSingle, Update, Delete). Callers higher up
/// (the CLI, or the Web API's exception-handling middleware) can catch
/// this specifically to show a clean "not found" message / 404 response,
/// instead of a generic error.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
