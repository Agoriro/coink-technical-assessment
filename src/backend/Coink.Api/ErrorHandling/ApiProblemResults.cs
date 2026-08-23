using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Coink.Api.ErrorHandling;

internal static class ApiProblemResults
{
    internal static IResult InvalidIdentifier(
        HttpContext httpContext,
        string parameterName,
        int maximumValue)
    {
        HttpValidationProblemDetails problemDetails = new(
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [parameterName] =
                [
                    $"The identifier must be between 1 and {maximumValue}.",
                ],
            })
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Detail = "Correct the invalid route value and try again.",
            Instance = httpContext.Request.Path,
        };

        AddExtensions(problemDetails, httpContext, "validation.failed");
        return TypedResults.Problem(problemDetails);
    }

    internal static IResult NotFound(
        HttpContext httpContext,
        string code,
        string detail)
    {
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found.",
            Detail = detail,
            Instance = httpContext.Request.Path,
        };

        AddExtensions(problemDetails, httpContext, code);
        return TypedResults.Problem(problemDetails);
    }

    private static void AddExtensions(
        ProblemDetails problemDetails,
        HttpContext httpContext,
        string code)
    {
        problemDetails.Extensions["code"] = code;
        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? httpContext.TraceIdentifier;
    }
}
