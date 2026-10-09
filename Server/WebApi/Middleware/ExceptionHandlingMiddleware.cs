using RepositoryContracts;

namespace WebApi.Middleware;

/// <summary>
/// Catches exceptions thrown anywhere further down the pipeline (i.e. in a
/// controller action, or in a repository it calls) and turns them into the
/// appropriate HTTP status code, so individual controller actions don't
/// need their own try-catch blocks.
/// - NotFoundException (an entity looked up by Id doesn't exist)    -> 404
/// - ValidationException (bad business-rule input, e.g. taken username) -> 400
/// - anything else (a genuine bug / unexpected failure)              -> 500
/// </summary>
public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException e)
        {
            logger.LogInformation(e, "Not found");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { error = e.Message });
        }
        catch (ValidationException e)
        {
            logger.LogInformation(e, "Validation failure");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = e.Message });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    }
}
