using System.Diagnostics;
using Coink.Application.Common;
using Microsoft.AspNetCore.Http.HttpResults;
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

    internal static IResult InvalidPositiveIdentifier(
        HttpContext httpContext,
        string parameterName)
    {
        HttpValidationProblemDetails problemDetails = new(
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [parameterName] = ["The identifier must be greater than 0."],
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

    internal static IResult FromApplicationError(
        HttpContext httpContext,
        ApplicationError error)
    {
        return error.Type switch
        {
            ApplicationErrorType.Validation => Validation(httpContext, error),
            ApplicationErrorType.NotFound => NotFound(
                httpContext,
                error.Code,
                error.Message),
            ApplicationErrorType.InvalidGeography => Problem(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Invalid geographic hierarchy.",
                error.Code,
                error.Message),
            _ => throw new InvalidOperationException(
                $"Unsupported application error type '{error.Type}'."),
        };
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

    private static ProblemHttpResult Validation(
        HttpContext httpContext,
        ApplicationError error)
    {
        HttpValidationProblemDetails problemDetails = new(
            error.ValidationErrors ?? new Dictionary<string, string[]>(StringComparer.Ordinal))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Detail = error.Message,
            Instance = httpContext.Request.Path,
        };

        AddExtensions(problemDetails, httpContext, error.Code);
        return TypedResults.Problem(problemDetails);
    }

    private static ProblemHttpResult Problem(
        HttpContext httpContext,
        int statusCode,
        string title,
        string code,
        string detail)
    {
        ProblemDetails problemDetails = new()
        {
            Status = statusCode,
            Title = title,
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
