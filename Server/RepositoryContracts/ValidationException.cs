namespace RepositoryContracts;

/// <summary>
/// Represents a business-rule failure on the incoming request/data itself
/// (e.g. a username that is already taken, or a request that references
/// another entity by an Id that doesn't exist). Distinct from
/// NotFoundException, which is about a missing entity addressed directly
/// by Id (e.g. GET /Users/42 where 42 doesn't exist).
/// Used by the Web API's exception-handling middleware to return 400
/// Bad Request instead of a generic 500.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}
