using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Lending.Service.Handlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not ValidationException validationException)
            return false;

        await Results.ValidationProblem(
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            .ExecuteAsync(httpContext);
        return true;
    }
}
