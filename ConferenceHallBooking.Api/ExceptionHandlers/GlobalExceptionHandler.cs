using ConferenceHallBooking.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHallBooking.Api.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string Rfc7807Type = "https://tools.ietf.org/html/rfc7807";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        var problemDetails = exception switch
        {
            ValidationException validationEx => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Validation error",
                validationEx.Message,
                traceId,
                [("errors", validationEx.Errors)]),

            NotFoundException notFound => CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Resource not found",
                notFound.Message,
                traceId),

            InvalidBookingTimeException timeEx => CreateProblemDetails(
                StatusCodes.Status400BadRequest, "Business rule validation error", timeEx.Message, traceId, [("errorCode", timeEx.ErrorCode)]),
            InvalidBaseHourlyRateException rateEx => CreateProblemDetails(
                StatusCodes.Status400BadRequest, "Business rule validation error", rateEx.Message, traceId, [("errorCode", rateEx.ErrorCode)]),
            InvalidEntityFieldException fieldEx => CreateProblemDetails(
                StatusCodes.Status400BadRequest, "Business rule validation error", fieldEx.Message, traceId, [("errorCode", fieldEx.ErrorCode)]),

            OptionsNotFoundException optionsNotFound => CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Options not found",
                optionsNotFound.Message,
                traceId),

            HallAlreadyBookedException bookedEx => CreateProblemDetails(
                StatusCodes.Status409Conflict, "Business rule violation", bookedEx.Message, traceId, [("errorCode", bookedEx.ErrorCode)]),
            HallOptionNotSupportedException optionEx => CreateProblemDetails(
                StatusCodes.Status409Conflict, "Business rule violation", optionEx.Message, traceId, [("errorCode", optionEx.ErrorCode)]),

            HallOptionInUseException optionInUseEx => CreateProblemDetails(
                StatusCodes.Status409Conflict, "Business rule violation", optionInUseEx.Message, traceId, [("errorCode", optionInUseEx.ErrorCode)]),

            HallNameAlreadyExistsException nameEx => CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Hall name conflict",
                nameEx.Message,
                traceId,
                [("errorCode", nameEx.ErrorCode)]),

            OptionNameAlreadyExistsException optionNameEx => CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Option name conflict",
                optionNameEx.Message,
                traceId,
                [("errorCode", optionNameEx.ErrorCode)]),

            EmailAlreadyExistsException emailEx => CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Email conflict",
                emailEx.Message,
                traceId,
                [("errorCode", emailEx.ErrorCode)]),

            InvalidCredentialsException credentialsEx => CreateProblemDetails(
                StatusCodes.Status401Unauthorized,
                "Invalid credentials",
                credentialsEx.Message,
                traceId,
                [("errorCode", credentialsEx.ErrorCode)]),

            BusinessRuleException businessEx => CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Business rule violation",
                businessEx.Message,
                traceId,
                [("errorCode", businessEx.ErrorCode)]),

            UnauthorizedAccessException unauthorizedEx => CreateProblemDetails(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                unauthorizedEx.Message,
                traceId),

            InvalidOperationException invalidOpEx => CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Conflict",
                invalidOpEx.Message,
                traceId),

            _ => CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred. Please try again later.",
                traceId)
        };

        var statusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        LogException(exception, statusCode);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        int status,
        string title,
        string detail,
        string traceId,
        IReadOnlyList<(string Key, object Value)>? extensions = null)
    {
        var problemDetails = new ProblemDetails
        {
            Type = Rfc7807Type,
            Title = title,
            Status = status,
            Detail = detail,
            Extensions = { ["traceId"] = traceId }
        };

        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        return problemDetails;
    }

    private void LogException(Exception exception, int statusCode)
    {
        if (statusCode >= 500)
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            logger.LogWarning(exception, "Client error: {Message}", exception.Message);
    }
}
