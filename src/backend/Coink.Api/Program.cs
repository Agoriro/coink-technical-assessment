using System.Diagnostics;
using Coink.Api.Configuration;
using Coink.Api.Endpoints;
using Coink.Api.ErrorHandling;
using Coink.Api.Middleware;
using Coink.Application;
using Coink.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_048_576;
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
        _ = context.ProblemDetails.Extensions.TryAdd(
            "traceId",
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
    };
});
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddConfiguredCors(builder.Configuration);
builder.Services.AddConfiguredRateLimiting(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "COINK User Registration API",
            Version = "v1",
            Description = "Versioned API for user registration and Colombian geography.",
        });
});

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStatusCodePages(static async statusCodeContext =>
{
    HttpContext httpContext = statusCodeContext.HttpContext;
    int statusCode = httpContext.Response.StatusCode;
    IProblemDetailsService problemDetailsService = httpContext.RequestServices
        .GetRequiredService<IProblemDetailsService>();

    _ = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
    {
        HttpContext = httpContext,
        ProblemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Instance = httpContext.Request.Path,
        },
    });
});
app.UseCors(ApiCorsConfiguration.PolicyName);
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "COINK API v1");
        options.RoutePrefix = "swagger";
    });
}

app.MapGeographyEndpoints();
app.MapUserEndpoints();

app.Run();

/// <summary>
/// Exposes the generated application entry point to future integration tests.
/// </summary>
public partial class Program;
