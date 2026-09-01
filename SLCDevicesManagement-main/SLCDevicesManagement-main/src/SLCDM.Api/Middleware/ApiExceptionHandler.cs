using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Exceptions;

namespace SLCDM.Api.Middleware;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, problem) = exception switch
        {
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Error de validacion",
                    Detail = "Uno o mas campos no son validos.",
                    Extensions = { ["errors"] = validation.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()) }
                }),
            UnauthorizedException unauthorized => (
                StatusCodes.Status401Unauthorized,
                new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "No autorizado",
                    Detail = unauthorized.Message
                }),
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "No encontrado",
                    Detail = notFound.Message
                }),
            ConflictException conflict => (
                StatusCodes.Status409Conflict,
                new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflicto",
                    Detail = conflict.Message
                }),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno",
                    Detail = "Ocurrio un error inesperado."
                })
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
